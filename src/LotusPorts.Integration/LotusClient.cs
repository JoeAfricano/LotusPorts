using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Lotus.Integration.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace LotusPorts.Integration;

public enum LotusConnectionState
{
    /// <summary>Lotus is not installed.</summary>
    NotFound,

    /// <summary>Lotus is installed but does not offer the integration yet (no registry entry): an older Lotus.</summary>
    NoIntegration,

    Connecting,
    Connected,

    /// <summary>Lotus speaks another protocol version: "Versão incompatível — atualize o Lotus".</summary>
    Incompatible,

    /// <summary>The program on the other end is not the Lotus registered on this PC: nothing is exchanged.</summary>
    Untrusted,

    /// <summary>Registered but not answering (closed, starting or busy). Retried quietly.</summary>
    Unavailable,
}

public sealed record LotusStatus(LotusConnectionState State, string? Version, IReadOnlyList<string> Capabilities)
{
    public static LotusStatus Of(LotusConnectionState state) => new(state, null, []);
}

/// <summary>Where Lotus says it listens (HKCU\Software\Lotus\Integration).</summary>
public sealed record LotusEndpoint(string PipeName, int ProtocolVersion, string? AppVersion, string ExePath);

public interface ILotusDiscovery
{
    LotusEndpoint? Find();

    bool LotusInstalled();
}

/// <summary>Confirms that the process serving the pipe is the registered Lotus executable.</summary>
public interface IPeerValidator
{
    bool IsTrusted(NamedPipeClientStream pipe, string expectedExePath);
}

public sealed class LotusUnavailableException(string message) : Exception(message);

public sealed class LotusRemoteException(int code, string message) : Exception(message)
{
    public int Code { get; } = code;
}

/// <summary>
/// The Lotus Ports side of the integration with Lotus (master prompt, section 12): named pipe, JSON-RPC with a 4-byte
/// length, handshake with capabilities, peer validation and 2-second timeouts. Lotus Ports works the same without it:
/// every call fails fast with <see cref="LotusUnavailableException"/> and nothing blocks the interface.
/// </summary>
public sealed class LotusClient(ILotusDiscovery discovery, IPeerValidator validator, ILogger<LotusClient> logger, string appVersion) : IAsyncDisposable
{
    /// <summary>Events Lotus Ports sends (Lotus may listen to them); methods it serves arrive with phase 6.</summary>
    public static readonly IReadOnlyList<string> OwnCapabilities =
    [
        Events.GameInstalled, Events.GameUpdated, Events.GameRolledBack, Events.GameRemoved, Events.PresetApplied, Events.SessionStarted, Events.SessionEnded,
    ];

    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonRpcMessage>> pending = new();
    private readonly SemaphoreSlim writeLock = new(1, 1);
    private readonly SemaphoreSlim connectLock = new(1, 1);
    private NamedPipeClientStream? pipe;
    private CancellationTokenSource? readLoop;
    private long nextId;

    public TimeSpan Timeout { get; init; } = Protocol.RequestTimeout;

    public LotusStatus Status { get; private set; } = LotusStatus.Of(LotusConnectionState.Unavailable);

    public event Action<LotusStatus>? StatusChanged;

    /// <summary>Notifications from Lotus (Dependencies.Changed, Driver.Changed, Profile.Changed…).</summary>
    public event Action<JsonRpcMessage>? EventReceived;

    public bool IsConnected => Status.State == LotusConnectionState.Connected;

    /// <summary>True only when Lotus announced this method or event in the handshake.</summary>
    public bool Supports(string capability) => IsConnected && Status.Capabilities.Contains(capability, StringComparer.Ordinal);

    /// <summary>One connection attempt: discovery, connect (2 s), peer check, handshake. Never throws.</summary>
    public async Task<LotusStatus> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsConnected) return Status;
            Close();
            var endpoint = discovery.Find();
            if (endpoint is null) return Set(LotusStatus.Of(discovery.LotusInstalled() ? LotusConnectionState.NoIntegration : LotusConnectionState.NotFound));
            if (endpoint.ProtocolVersion != Protocol.Version) return Set(new(LotusConnectionState.Incompatible, endpoint.AppVersion, []));
            Set(LotusStatus.Of(LotusConnectionState.Connecting));

            var client = new NamedPipeClientStream(".", endpoint.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                using var connectTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectTimeout.CancelAfter(Timeout);
                await client.ConnectAsync(connectTimeout.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException or TimeoutException or IOException or UnauthorizedAccessException)
            {
                await client.DisposeAsync().ConfigureAwait(false);
                return Set(LotusStatus.Of(LotusConnectionState.Unavailable));
            }

            if (!validator.IsTrusted(client, endpoint.ExePath))
            {
                logger.LogWarning("Integration pipe is served by a program that is not the registered Lotus");
                await client.DisposeAsync().ConfigureAwait(false);
                return Set(LotusStatus.Of(LotusConnectionState.Untrusted));
            }

            pipe = client;
            readLoop = new CancellationTokenSource();
            _ = ReadLoopAsync(client, readLoop.Token);
            try
            {
                var reply = await SendAsync(Methods.Handshake, new HandshakeParams("Lotus Ports", appVersion, Protocol.Version, OwnCapabilities), cancellationToken).ConfigureAwait(false);
                var result = reply.ResultAs<HandshakeResult>();
                if (result is null) throw new LotusUnavailableException("Empty handshake.");
                if (result.ProtocolVersion != Protocol.Version)
                {
                    Close();
                    return Set(new(LotusConnectionState.Incompatible, result.AppVersion, []));
                }
                return Set(new(LotusConnectionState.Connected, result.AppVersion, result.Capabilities ?? []));
            }
            catch (LotusRemoteException ex) when (ex.Code == JsonRpcErrorCodes.IncompatibleVersion)
            {
                Close();
                return Set(LotusStatus.Of(LotusConnectionState.Incompatible));
            }
            catch (Exception ex) when (ex is LotusUnavailableException or LotusRemoteException or IOException or ObjectDisposedException)
            {
                logger.LogInformation(ex, "Lotus handshake failed");
                Close();
                return Set(LotusStatus.Of(LotusConnectionState.Unavailable));
            }
        }
        finally
        {
            connectLock.Release();
        }
    }

    /// <summary>
    /// Keeps the connection in the background: reconnects quietly after Lotus closes or restarts. Not found / no
    /// integration are checked again less often, so an installed Lotus is noticed without a restart.
    /// </summary>
    public async Task RunAsync(TimeSpan retryEvery, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected) await ConnectAsync(cancellationToken).ConfigureAwait(false);
            try { await Task.Delay(retryEvery, cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
        }
    }

    /// <summary>Calls a method Lotus announced. Throws <see cref="NotSupportedException"/> when it did not.</summary>
    public async Task<TResult?> RequestAsync<TParams, TResult>(string method, TParams parameters, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) throw new LotusUnavailableException("Lotus is not connected.");
        if (!Supports(method)) throw new NotSupportedException($"Lotus did not announce {method}.");
        var reply = await SendAsync(method, parameters, cancellationToken).ConfigureAwait(false);
        return reply.ResultAs<TResult>();
    }

    /// <summary>Sends an event if Lotus listens to it; never throws (a lost event is fine: each side re-reads on reconnect).</summary>
    public async Task NotifyAsync<T>(string eventName, T body)
    {
        if (!Supports(eventName) || pipe is not { } stream) return;
        try
        {
            await writeLock.WaitAsync().ConfigureAwait(false);
            try { await MessageFraming.WriteAsync(stream, JsonRpcMessage.Notification(eventName, body)).ConfigureAwait(false); }
            finally { writeLock.Release(); }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or MessageFramingException)
        {
            Lost();
        }
    }

    private async Task<JsonRpcMessage> SendAsync<T>(string method, T parameters, CancellationToken cancellationToken)
    {
        var stream = pipe ?? throw new LotusUnavailableException("Lotus is not connected.");
        var id = Interlocked.Increment(ref nextId).ToString(System.Globalization.CultureInfo.InvariantCulture);
        var waiter = new TaskCompletionSource<JsonRpcMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending[id] = waiter;
        try
        {
            await writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try { await MessageFraming.WriteAsync(stream, JsonRpcMessage.Request(id, method, parameters), cancellationToken).ConfigureAwait(false); }
            finally { writeLock.Release(); }

            var reply = await waiter.Task.WaitAsync(Timeout, cancellationToken).ConfigureAwait(false);
            if (reply.Error is { } error) throw new LotusRemoteException(error.Code, error.Message);
            return reply;
        }
        catch (TimeoutException)
        {
            throw new LotusUnavailableException($"Lotus did not answer {method} in time.");
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or MessageFramingException)
        {
            Lost();
            throw new LotusUnavailableException("The connection to Lotus was lost.");
        }
        finally
        {
            pending.TryRemove(id, out _);
        }
    }

    private async Task ReadLoopAsync(NamedPipeClientStream stream, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await MessageFraming.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
                if (message is null) break;
                if (message.IsResponse && message.Id is { } id && pending.TryGetValue(id, out var waiter)) waiter.TrySetResult(message);
                else if (message.IsNotification)
                {
                    if (message.Method == Events.LotusShuttingDown) break;
                    try { EventReceived?.Invoke(message); }
                    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { logger.LogDebug(ex, "Event handler failed"); }
                }
                else if (message.IsRequest && message.Id is { } requestId)
                {
                    // Methods Lotus Ports serves arrive with phase 6; anything else is refused, never run.
                    await writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try { await MessageFraming.WriteAsync(stream, JsonRpcMessage.Failure(requestId, JsonRpcErrorCodes.MethodNotFound, "Method not offered."), cancellationToken).ConfigureAwait(false); }
                    finally { writeLock.Release(); }
                }
            }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or MessageFramingException or OperationCanceledException)
        {
            logger.LogDebug(ex, "Lotus connection closed");
        }
        if (!cancellationToken.IsCancellationRequested && ReferenceEquals(pipe, stream)) Lost();
    }

    private void Lost()
    {
        Close();
        Set(LotusStatus.Of(LotusConnectionState.Unavailable));
    }

    private void Close()
    {
        readLoop?.Cancel();
        readLoop = null;
        var old = Interlocked.Exchange(ref pipe, null);
        old?.Dispose();
        foreach (var (_, waiter) in pending) waiter.TrySetException(new LotusUnavailableException("The connection to Lotus was lost."));
        pending.Clear();
    }

    private LotusStatus Set(LotusStatus status)
    {
        var changed = status != Status;
        Status = status;
        if (changed) StatusChanged?.Invoke(status);
        return status;
    }

    public ValueTask DisposeAsync()
    {
        Close();
        writeLock.Dispose();
        connectLock.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>Reads HKCU\Software\Lotus\Integration, written by Lotus when it offers the integration.</summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryLotusDiscovery : ILotusDiscovery
{
    public LotusEndpoint? Find()
    {
        using var key = Registry.CurrentUser.OpenSubKey(Protocol.LotusRegistryKey);
        if (key?.GetValue(RegistryValueNames.PipeName) is not string pipeName || key.GetValue(RegistryValueNames.ExePath) is not string exe) return null;
        var version = key.GetValue(RegistryValueNames.ProtocolVersion) switch
        {
            int i => i,
            string s when int.TryParse(s, out var p) => p,
            _ => 0,
        };
        return new LotusEndpoint(pipeName, version, key.GetValue(RegistryValueNames.AppVersion) as string, exe);
    }

    public bool LotusInstalled()
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var uninstall = hive.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstall is null) continue;
            foreach (var name in uninstall.GetSubKeyNames())
            {
                using var app = uninstall.OpenSubKey(name);
                if (app?.GetValue("DisplayName") as string == "Lotus") return true;
            }
        }
        return false;
    }
}

/// <summary>The pipe's server process must be the executable Lotus registered (section 12.3).</summary>
[SupportedOSPlatform("windows")]
public sealed partial class ProcessPeerValidator : IPeerValidator
{
    private const uint QueryLimitedInformation = 0x1000;

    public bool IsTrusted(NamedPipeClientStream pipe, string expectedExePath)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        if (!GetNamedPipeServerProcessId(pipe.SafePipeHandle, out var pid)) return false;
        return ProcessPath(pid) is { } path
            && string.Equals(Path.GetFullPath(path), Path.GetFullPath(expectedExePath), StringComparison.OrdinalIgnoreCase);
    }

    public static string? ProcessPath(uint pid)
    {
        using var process = OpenProcess(QueryLimitedInformation, false, pid);
        if (process.IsInvalid) return null;
        var buffer = new char[32768];
        var size = (uint)buffer.Length;
        return QueryFullProcessImageName(process, 0, buffer, ref size) ? new string(buffer, 0, (int)size) : null;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint serverProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint processId);

    [LibraryImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool QueryFullProcessImageName(SafeProcessHandle process, uint flags, [Out] char[] buffer, ref uint size);
}
