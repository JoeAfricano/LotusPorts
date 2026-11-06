using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using Lotus.Integration.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace LotusPorts.Integration;

/// <summary>What Lotus may ask Lotus Ports (section 12.7 and 12.8), implemented by the app.</summary>
public interface IPortsHandler
{
    /// <summary>The switch for this method is on (Ports.* → "listar e abrir meus jogos", Journal.Query → "compartilhar o histórico").</summary>
    bool IsAllowed(string method);

    Task<PortsListGamesResult> ListGamesAsync();

    /// <summary>Null when the id is not a game of the catalog.</summary>
    Task<PortsLaunchResult?> LaunchAsync(string gameId);

    /// <summary>False when the id is not a game of the catalog.</summary>
    Task<bool> OpenAsync(string? gameId);

    Task<JournalQueryResult> JournalAsync(int limit);

    /// <summary>Events from Lotus (Driver.Changed, Dependencies.Changed, Profile.Changed…).</summary>
    void OnEvent(JsonRpcMessage message);
}

/// <summary>Confirms the connecting process is the registered Lotus.</summary>
public interface IClientValidator
{
    bool IsTrusted(NamedPipeServerStream pipe);
}

/// <summary>
/// Lotus Ports' own pipe (\\.\pipe\lotusports.integration.v1), only for the current user and only for the registered
/// Lotus executable. The first message must be the handshake; afterwards only the four methods below are answered, each
/// behind its switch. Nothing else runs: unknown methods get -32601, invalid parameters -32602.
/// </summary>
public sealed class PortsServer(string pipeName, IPortsHandler handler, IClientValidator validator, ILogger<PortsServer> logger, string appVersion) : IAsyncDisposable
{
    public static readonly IReadOnlyList<string> Capabilities =
    [
        Methods.PortsListGames, Methods.PortsLaunch, Methods.PortsOpen, Methods.JournalQuery,
        Events.DependenciesChanged, Events.DriverChanged, Events.ProfileChanged, Events.LotusShuttingDown,
    ];

    private readonly CancellationTokenSource stop = new();
    private Task? accept;

    public void Start() => accept = Task.Run(() => AcceptLoopAsync(stop.Token));

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream server;
            try
            {
                server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            }
            catch (IOException ex)
            {
                // Another Lotus Ports already serves the pipe: this one stays quiet.
                logger.LogInformation(ex, "Integration pipe already in use");
                return;
            }
            try
            {
                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                await server.DisposeAsync().ConfigureAwait(false);
                return;
            }
            if (!validator.IsTrusted(server))
            {
                logger.LogWarning("Refused an integration connection from a program that is not the registered Lotus");
                await server.DisposeAsync().ConfigureAwait(false);
                continue;
            }
            _ = ServeAsync(server, cancellationToken);
        }
    }

    private async Task ServeAsync(NamedPipeServerStream stream, CancellationToken cancellationToken)
    {
        await using var _ = stream;
        var writeLock = new SemaphoreSlim(1, 1);
        var greeted = false;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await MessageFraming.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
                if (message is null) break;
                if (message.IsNotification)
                {
                    if (!greeted) break;
                    if (message.Method == Events.LotusShuttingDown) break;
                    handler.OnEvent(message);
                    continue;
                }
                if (!message.IsRequest) continue;
                JsonRpcMessage reply;
                if (!greeted)
                {
                    if (message.Method != Methods.Handshake)
                    {
                        await Write(JsonRpcMessage.Failure(message.Id!, JsonRpcErrorCodes.InvalidRequest, "Handshake first.")).ConfigureAwait(false);
                        break;
                    }
                    var hello = Params<HandshakeParams>(message);
                    if (hello is null || hello.ProtocolVersion != Protocol.Version)
                    {
                        await Write(JsonRpcMessage.Failure(message.Id!, JsonRpcErrorCodes.IncompatibleVersion, "Protocol version not supported.")).ConfigureAwait(false);
                        break;
                    }
                    greeted = true;
                    reply = JsonRpcMessage.Success(message.Id!, new HandshakeResult("Lotus Ports", appVersion, Protocol.Version, Capabilities));
                }
                else reply = await HandleAsync(message).ConfigureAwait(false);
                await Write(reply).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or MessageFramingException or OperationCanceledException)
        {
            logger.LogDebug(ex, "Integration connection closed");
        }

        async Task Write(JsonRpcMessage reply)
        {
            await writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try { await MessageFraming.WriteAsync(stream, reply, cancellationToken).ConfigureAwait(false); }
            finally { writeLock.Release(); }
        }
    }

    private async Task<JsonRpcMessage> HandleAsync(JsonRpcMessage message)
    {
        var id = message.Id!;
        var method = message.Method!;
        if (!Capabilities.Contains(method) || method.StartsWith("Ports.", StringComparison.Ordinal) is false && method != Methods.JournalQuery)
            return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.MethodNotFound, "Method not offered.");
        if (!handler.IsAllowed(method)) return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.PermissionDisabled, "Turned off in Lotus Ports.");
        try
        {
            switch (method)
            {
                case Methods.PortsListGames:
                    return JsonRpcMessage.Success(id, await handler.ListGamesAsync().ConfigureAwait(false));
                case Methods.PortsLaunch:
                    if (Params<PortsLaunchParams>(message) is not { GameId: { Length: > 0 and <= 100 } gameId } || !IsId(gameId))
                        return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.InvalidParams, "A catalog game id is required.");
                    return await handler.LaunchAsync(gameId).ConfigureAwait(false) is { } launched
                        ? JsonRpcMessage.Success(id, launched)
                        : JsonRpcMessage.Failure(id, JsonRpcErrorCodes.UnknownGame, "Unknown game.");
                case Methods.PortsOpen:
                    var open = Params<PortsOpenParams>(message);
                    if (open?.GameId is { } openId && !IsId(openId)) return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.InvalidParams, "Invalid game id.");
                    return await handler.OpenAsync(open?.GameId).ConfigureAwait(false)
                        ? JsonRpcMessage.Success(id, new { opened = true })
                        : JsonRpcMessage.Failure(id, JsonRpcErrorCodes.UnknownGame, "Unknown game.");
                case Methods.JournalQuery:
                    var limit = Params<JournalQueryParams>(message)?.Limit ?? 50;
                    return JsonRpcMessage.Success(id, await handler.JournalAsync(Math.Clamp(limit, 1, 500)).ConfigureAwait(false));
                default:
                    return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.MethodNotFound, "Method not offered.");
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or ArgumentException)
        {
            logger.LogWarning(ex, "Integration request {Method} failed", method);
            return JsonRpcMessage.Failure(id, JsonRpcErrorCodes.InternalError, "Lotus Ports could not answer.");
        }
    }

    private static T? Params<T>(JsonRpcMessage message)
    {
        try { return message.ParamsAs<T>(); }
        catch (JsonException) { return default; }
    }

    /// <summary>Catalog ids are lowercase words with dashes; nothing else (no paths) is accepted.</summary>
    private static bool IsId(string value) => value.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-');

    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync().ConfigureAwait(false);
        if (accept is not null) await accept.ConfigureAwait(false);
        stop.Dispose();
    }
}

/// <summary>The connecting process must be the executable Lotus registered (HKCU\Software\Lotus\Integration\ExePath).</summary>
[SupportedOSPlatform("windows")]
public sealed partial class LotusClientValidator(ILotusDiscovery discovery) : IClientValidator
{
    public bool IsTrusted(NamedPipeServerStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        if (discovery.Find() is not { } lotus || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out var pid)) return false;
        return ProcessPeerValidator.ProcessPath(pid) is { } path
            && string.Equals(Path.GetFullPath(path), Path.GetFullPath(lotus.ExePath), StringComparison.OrdinalIgnoreCase);
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);
}

/// <summary>Writes HKCU\Software\LotusPorts\Integration so Lotus can find Lotus Ports (and removes the pipe name on exit).</summary>
[SupportedOSPlatform("windows")]
public static class PortsRegistration
{
    public static void Register(string appVersion, string exePath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(Protocol.PortsRegistryKey);
        key.SetValue(RegistryValueNames.PipeName, Protocol.PortsPipeName);
        key.SetValue(RegistryValueNames.ProtocolVersion, Protocol.Version, RegistryValueKind.DWord);
        key.SetValue(RegistryValueNames.AppVersion, appVersion);
        key.SetValue(RegistryValueNames.ExePath, exePath);
    }

    public static void Unregister()
    {
        using var key = Registry.CurrentUser.OpenSubKey(Protocol.PortsRegistryKey, writable: true);
        key?.DeleteValue(RegistryValueNames.PipeName, throwOnMissingValue: false);
    }
}
