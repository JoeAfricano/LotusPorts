using System.IO.Pipes;
using Lotus.Integration.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Integration.Tests;

/// <summary>Phase 6: Lotus (simulated here as a pipe client) lists and opens Lotus Ports games, only through catalog ids.</summary>
public sealed class PortsServerTests
{
    private sealed class Handler : IPortsHandler
    {
        public HashSet<string> Disallowed { get; } = [];
        public List<string> Launched { get; } = [];
        public List<string?> Opened { get; } = [];
        public List<string> Events { get; } = [];

        public bool IsAllowed(string method) => !Disallowed.Contains(method);

        public Task<PortsListGamesResult> ListGamesAsync() => Task.FromResult(new PortsListGamesResult(
        [
            new PortsGame("zelda-majoras-mask", "The Legend of Zelda: Majora's Mask", "port:zelda-majoras-mask", "N64", PortsGameState.Ready, "v1.2.2", null, true, null),
        ]));

        public Task<PortsLaunchResult?> LaunchAsync(string gameId)
        {
            if (gameId != "zelda-majoras-mask") return Task.FromResult<PortsLaunchResult?>(null);
            Launched.Add(gameId);
            return Task.FromResult<PortsLaunchResult?>(new PortsLaunchResult(true, null));
        }

        public Task<bool> OpenAsync(string? gameId)
        {
            Opened.Add(gameId);
            return Task.FromResult(gameId is null or "zelda-majoras-mask");
        }

        public Task<JournalQueryResult> JournalAsync(int limit) => Task.FromResult(new JournalQueryResult(
            [new JournalEntryDto(1, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "Install", "zelda64-recompiled", "Success", "v1.2.1 (Ok)", Guid.NewGuid().ToString())]));

        public void OnEvent(JsonRpcMessage message)
        {
            lock (Events) Events.Add(message.Method!);
        }
    }

    private sealed class Validator(bool trusted) : IClientValidator
    {
        public bool IsTrusted(NamedPipeServerStream pipe) => trusted;
    }

    private static (PortsServer Server, Handler Handler, string Pipe) Start(bool trusted = true)
    {
        var pipe = "lotusports-test-" + Guid.NewGuid().ToString("N");
        var handler = new Handler();
        var server = new PortsServer(pipe, handler, new Validator(trusted), NullLogger<PortsServer>.Instance, "0.1.0");
        server.Start();
        return (server, handler, pipe);
    }

    private static async Task<NamedPipeClientStream> ConnectAsync(string pipe)
    {
        var client = new NamedPipeClientStream(".", pipe, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(3000);
        return client;
    }

    private static async Task<JsonRpcMessage?> Ask<T>(Stream stream, string id, string method, T parameters)
    {
        await MessageFraming.WriteAsync(stream, JsonRpcMessage.Request(id, method, parameters));
        return await MessageFraming.ReadAsync(stream).WaitAsync(TimeSpan.FromSeconds(3));
    }

    private static Task<JsonRpcMessage?> Hello(Stream stream, int version = Protocol.Version) =>
        Ask(stream, "0", Methods.Handshake, new HandshakeParams("Lotus", "1.2.0", version, [Methods.SessionStart]));

    [Fact]
    public async Task Lotus_lists_and_opens_games_after_the_handshake()
    {
        var (server, handler, pipe) = Start();
        await using var _ = server;
        await using var lotus = await ConnectAsync(pipe);

        var hello = (await Hello(lotus))!.ResultAs<HandshakeResult>()!;
        hello.Should().Match<HandshakeResult>(h => h.AppName == "Lotus Ports" && h.ProtocolVersion == Protocol.Version);
        hello.Capabilities.Should().Contain([Methods.PortsListGames, Methods.PortsLaunch, Methods.PortsOpen, Methods.JournalQuery]);

        var games = (await Ask(lotus, "1", Methods.PortsListGames, new { }))!.ResultAs<PortsListGamesResult>()!;
        games.Games.Should().ContainSingle().Which.State.Should().Be(PortsGameState.Ready);

        (await Ask(lotus, "2", Methods.PortsLaunch, new PortsLaunchParams("zelda-majoras-mask")))!.ResultAs<PortsLaunchResult>()!.Started.Should().BeTrue();
        handler.Launched.Should().Equal("zelda-majoras-mask");
        (await Ask(lotus, "3", Methods.PortsOpen, new PortsOpenParams(null)))!.Error.Should().BeNull();
        (await Ask(lotus, "4", Methods.JournalQuery, new JournalQueryParams(10)))!.ResultAs<JournalQueryResult>()!.Entries.Should().ContainSingle();

        await MessageFraming.WriteAsync(lotus, JsonRpcMessage.Notification(Events.DriverChanged, new DriverChangedEvent("GPU", "560", DateTimeOffset.UtcNow)));
        for (var i = 0; i < 30 && handler.Events.Count == 0; i++) await Task.Delay(100);
        handler.Events.Should().Equal(Events.DriverChanged);
    }

    [Fact]
    public async Task Paths_unknown_games_and_unknown_methods_are_refused()
    {
        var (server, handler, pipe) = Start();
        await using var _ = server;
        await using var lotus = await ConnectAsync(pipe);
        await Hello(lotus);

        (await Ask(lotus, "1", Methods.PortsLaunch, new PortsLaunchParams(@"C:\Windows\System32\cmd.exe")))!.Error!.Code.Should().Be(JsonRpcErrorCodes.InvalidParams);
        (await Ask(lotus, "2", Methods.PortsLaunch, new PortsLaunchParams("no-such-game")))!.Error!.Code.Should().Be(JsonRpcErrorCodes.UnknownGame);
        (await Ask(lotus, "3", "Process.Start", new { file = "cmd.exe" }))!.Error!.Code.Should().Be(JsonRpcErrorCodes.MethodNotFound);
        (await Ask(lotus, "4", Methods.PortsLaunch, new { }))!.Error!.Code.Should().Be(JsonRpcErrorCodes.InvalidParams);
        handler.Launched.Should().BeEmpty();
    }

    [Fact]
    public async Task A_switch_turned_off_answers_permission_disabled()
    {
        var (server, handler, pipe) = Start();
        await using var _ = server;
        handler.Disallowed.Add(Methods.PortsListGames);
        await using var lotus = await ConnectAsync(pipe);
        await Hello(lotus);

        (await Ask(lotus, "1", Methods.PortsListGames, new { }))!.Error!.Code.Should().Be(JsonRpcErrorCodes.PermissionDisabled);
    }

    [Fact]
    public async Task Without_handshake_or_with_another_version_the_connection_ends()
    {
        var (server, _, pipe) = Start();
        await using var _s = server;

        await using (var early = await ConnectAsync(pipe))
        {
            (await Ask(early, "1", Methods.PortsListGames, new { }))!.Error!.Code.Should().Be(JsonRpcErrorCodes.InvalidRequest);
            (await MessageFraming.ReadAsync(early).WaitAsync(TimeSpan.FromSeconds(3))).Should().BeNull("the server closed it");
        }
        await using var future = await ConnectAsync(pipe);
        (await Hello(future, version: 2))!.Error!.Code.Should().Be(JsonRpcErrorCodes.IncompatibleVersion);
    }

    [Fact]
    public async Task A_program_that_is_not_lotus_gets_nothing()
    {
        var (server, handler, pipe) = Start(trusted: false);
        await using var _ = server;
        await using var impostor = await ConnectAsync(pipe);
        JsonRpcMessage? answer = null;
        try
        {
            await MessageFraming.WriteAsync(impostor, JsonRpcMessage.Request("0", Methods.Handshake, new HandshakeParams("Lotus", "1", 1, [])));
            answer = await MessageFraming.ReadAsync(impostor).WaitAsync(TimeSpan.FromSeconds(3));
        }
        catch (IOException)
        {
            // The server already closed the pipe: also a refusal.
        }
        answer.Should().BeNull("the server dropped the connection without answering");
        handler.Launched.Should().BeEmpty();
    }
}
