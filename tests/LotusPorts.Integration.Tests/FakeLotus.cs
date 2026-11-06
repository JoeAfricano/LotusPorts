using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Lotus.Integration.Contracts;

namespace LotusPorts.Integration.Tests;

/// <summary>
/// A simulated Lotus (master prompt, section 12.12): a real named-pipe server in the test process, answering the
/// handshake with the capabilities and version given, and the methods with the handlers given.
/// </summary>
public sealed class FakeLotus : IAsyncDisposable
{
    private readonly CancellationTokenSource stop = new();
    private readonly List<Task> sessions = [];
    private NamedPipeServerStream? current;

    public string PipeName { get; } = "lotus-test-" + Guid.NewGuid().ToString("N");

    public int ProtocolVersion { get; init; } = Protocol.Version;

    public IReadOnlyList<string> Capabilities { get; init; } = [];

    /// <summary>Method → handler returning the result object (or throwing to answer with an error).</summary>
    public Dictionary<string, Func<JsonElement?, object?>> Handlers { get; } = [];

    /// <summary>When set, the handshake is answered with garbage instead of JSON.</summary>
    public bool SendGarbage { get; init; }

    /// <summary>When set, requests other than the handshake are never answered.</summary>
    public bool Silent { get; init; }

    public List<JsonRpcMessage> Received { get; } = [];

    public HandshakeParams? Handshake { get; private set; }

    public LotusEndpoint Endpoint => new(PipeName, ProtocolVersion, "1.2.0", Environment.ProcessPath!);

    public void Start()
    {
        sessions.Add(Task.Run(async () =>
        {
            while (!stop.IsCancellationRequested)
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 4, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                try { await server.WaitForConnectionAsync(stop.Token); }
                catch (OperationCanceledException) { await server.DisposeAsync(); break; }
                current = server;
                _ = ServeAsync(server);
            }
        }));
    }

    private async Task ServeAsync(NamedPipeServerStream server)
    {
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var message = await MessageFraming.ReadAsync(server, stop.Token);
                if (message is null) break;
                lock (Received) Received.Add(message);
                if (!message.IsRequest) continue;
                if (message.Method == Methods.Handshake)
                {
                    Handshake = message.ParamsAs<HandshakeParams>();
                    if (SendGarbage)
                    {
                        var junk = Encoding.UTF8.GetBytes("{not json");
                        await server.WriteAsync(BitConverter.GetBytes(junk.Length), stop.Token);
                        await server.WriteAsync(junk, stop.Token);
                        await server.FlushAsync(stop.Token);
                        continue;
                    }
                    await MessageFraming.WriteAsync(server, JsonRpcMessage.Success(message.Id!, new HandshakeResult("Lotus", "1.2.0", ProtocolVersion, Capabilities)), stop.Token);
                    continue;
                }
                if (Silent) continue;
                JsonRpcMessage reply;
                if (Handlers.TryGetValue(message.Method!, out var handler))
                {
                    try { reply = JsonRpcMessage.Success(message.Id!, handler(message.Params)); }
                    catch (InvalidOperationException ex) { reply = JsonRpcMessage.Failure(message.Id!, JsonRpcErrorCodes.InternalError, ex.Message); }
                }
                else reply = JsonRpcMessage.Failure(message.Id!, JsonRpcErrorCodes.MethodNotFound, "unknown");
                await MessageFraming.WriteAsync(server, reply, stop.Token);
            }
        }
        catch (Exception ex) when (ex is IOException or OperationCanceledException or ObjectDisposedException or MessageFramingException)
        {
        }
    }

    /// <summary>Sends an event to the connected client.</summary>
    public Task SendEventAsync<T>(string name, T body) => MessageFraming.WriteAsync(current!, JsonRpcMessage.Notification(name, body));

    /// <summary>Drops the current connection (Lotus closed) while the server keeps listening (Lotus restarted).</summary>
    public void DropConnection() => current?.Dispose();

    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync();
        current?.Dispose();
        try { await Task.WhenAll(sessions).WaitAsync(TimeSpan.FromSeconds(3)); } catch (TimeoutException) { }
        stop.Dispose();
    }
}

public sealed class FixedDiscovery(LotusEndpoint? endpoint, bool installed = true) : ILotusDiscovery
{
    public LotusEndpoint? Find() => endpoint;

    public bool LotusInstalled() => installed;
}

public sealed class FixedValidator(bool trusted) : IPeerValidator
{
    public bool IsTrusted(NamedPipeClientStream pipe, string expectedExePath) => trusted;
}
