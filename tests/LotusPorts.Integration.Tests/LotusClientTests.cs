using Lotus.Integration.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Integration.Tests;

/// <summary>Phase 5 acceptance: works with Lotus absent, a full simulated Lotus, and one without some capabilities.</summary>
public sealed class LotusClientTests
{
    private static LotusClient Client(ILotusDiscovery discovery, bool trusted = true, TimeSpan? timeout = null) =>
        new(discovery, new FixedValidator(trusted), NullLogger<LotusClient>.Instance, "0.1.0") { Timeout = timeout ?? TimeSpan.FromSeconds(2) };

    private static readonly string[] Full =
        [Methods.DependenciesCheck, Methods.DependenciesRequestInstall, Methods.DiagnosticsRunSystemChecks, Methods.JournalQueryRecent, Methods.SessionStart, Methods.SessionEnd, Events.SessionStarted];

    [Fact]
    public async Task Without_lotus_everything_fails_fast_and_says_why()
    {
        await using var notInstalled = Client(new FixedDiscovery(null, installed: false));
        (await notInstalled.ConnectAsync()).State.Should().Be(LotusConnectionState.NotFound);

        await using var oldLotus = Client(new FixedDiscovery(null, installed: true));
        (await oldLotus.ConnectAsync()).State.Should().Be(LotusConnectionState.NoIntegration);

        var act = () => oldLotus.RequestAsync<DependenciesCheckParams, DependenciesCheckResult>(Methods.DependenciesCheck, new([DependencyIds.VulkanRuntime]));
        await act.Should().ThrowAsync<LotusUnavailableException>();
        await oldLotus.NotifyAsync(Events.SessionStarted, new { }); // no exception, nothing sent
    }

    [Fact]
    public async Task Registered_but_closed_lotus_is_unavailable_within_the_timeout()
    {
        await using var client = Client(new FixedDiscovery(new LotusEndpoint("lotus-nobody-" + Guid.NewGuid().ToString("N"), 1, "1.0", "C:\\x.exe")), timeout: TimeSpan.FromMilliseconds(500));
        var started = DateTime.UtcNow;
        (await client.ConnectAsync()).State.Should().Be(LotusConnectionState.Unavailable);
        (DateTime.UtcNow - started).Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Full_lotus_handshakes_and_answers_announced_methods()
    {
        await using var lotus = new FakeLotus { Capabilities = Full };
        lotus.Handlers[Methods.DiagnosticsRunSystemChecks] = _ => new SystemChecksResult([new SystemCheckItem("gpu-driver", CheckStatus.Warning, "Driver de vídeo", "Driver de 2023.")]);
        lotus.Handlers[Methods.SessionStart] = p => new SessionStartResult("s-" + p!.Value.GetProperty("portId").GetString());
        lotus.Start();
        await using var client = Client(new FixedDiscovery(lotus.Endpoint));

        var status = await client.ConnectAsync();

        status.Should().Match<LotusStatus>(s => s.State == LotusConnectionState.Connected && s.Version == "1.2.0");
        lotus.Handshake!.Should().Match<HandshakeParams>(h => h.AppName == "Lotus Ports" && h.ProtocolVersion == Protocol.Version && h.Capabilities.Contains(Events.SessionEnded));
        var checks = await client.RequestAsync<SystemChecksParams, SystemChecksResult>(Methods.DiagnosticsRunSystemChecks, new(["Vulkan"], null));
        checks!.Items.Should().ContainSingle().Which.State.Should().Be(CheckStatus.Warning);
        var session = await client.RequestAsync<SessionStartParams, SessionStartResult>(Methods.SessionStart, new("port:zelda", "Zelda", "zelda64", "v1", "C:\\g.exe", 1, null));
        session!.SessionId.Should().Be("s-zelda64");
    }

    [Fact]
    public async Task Missing_capabilities_are_not_called_and_the_feature_hides()
    {
        await using var lotus = new FakeLotus { Capabilities = [Methods.SessionStart] };
        lotus.Start();
        await using var client = Client(new FixedDiscovery(lotus.Endpoint));
        await client.ConnectAsync();

        client.Supports(Methods.SessionStart).Should().BeTrue();
        client.Supports(Methods.DiagnosticsRunSystemChecks).Should().BeFalse();
        var act = () => client.RequestAsync<SystemChecksParams, SystemChecksResult>(Methods.DiagnosticsRunSystemChecks, new([], null));
        await act.Should().ThrowAsync<NotSupportedException>();
        await client.NotifyAsync(Events.GameInstalled, new GameEvent("g", "p", "v1"));
        await Task.Delay(200);
        lotus.Received.Select(m => m.Method).Should().Equal(Methods.Handshake);
    }

    [Fact]
    public async Task Incompatible_version_is_reported_without_using_lotus()
    {
        await using var lotus = new FakeLotus { ProtocolVersion = 2, Capabilities = Full };
        lotus.Start();
        await using var registered = Client(new FixedDiscovery(lotus.Endpoint));
        (await registered.ConnectAsync()).State.Should().Be(LotusConnectionState.Incompatible, "the registry already says version 2");

        await using var answered = Client(new FixedDiscovery(lotus.Endpoint with { ProtocolVersion = 1 }));
        (await answered.ConnectAsync()).State.Should().Be(LotusConnectionState.Incompatible, "the handshake says version 2");
    }

    [Fact]
    public async Task A_program_that_is_not_the_registered_lotus_is_refused()
    {
        await using var impostor = new FakeLotus { Capabilities = Full };
        impostor.Start();
        await using var client = Client(new FixedDiscovery(impostor.Endpoint), trusted: false);
        (await client.ConnectAsync()).State.Should().Be(LotusConnectionState.Untrusted);
        await Task.Delay(200);
        impostor.Received.Should().BeEmpty("nothing is sent before the peer is confirmed");
    }

    [Fact]
    public async Task Real_peer_check_trusts_only_the_registered_program()
    {
        await using var lotus = new FakeLotus { Capabilities = Full };
        lotus.Start();
        var validator = new ProcessPeerValidator();
        await using (var right = new LotusClient(new FixedDiscovery(lotus.Endpoint), validator, NullLogger<LotusClient>.Instance, "0.1.0"))
            (await right.ConnectAsync()).State.Should().Be(LotusConnectionState.Connected, "the server runs in this very process");
        await using var wrong = new LotusClient(new FixedDiscovery(lotus.Endpoint with { ExePath = @"C:\Program Files\Lotus\Lotus.App.exe" }), validator, NullLogger<LotusClient>.Instance, "0.1.0");
        (await wrong.ConnectAsync()).State.Should().Be(LotusConnectionState.Untrusted);
    }

    [Fact]
    public async Task A_silent_lotus_times_out_and_malformed_replies_drop_the_connection()
    {
        await using var silent = new FakeLotus { Capabilities = Full, Silent = true };
        silent.Start();
        await using var client = Client(new FixedDiscovery(silent.Endpoint), timeout: TimeSpan.FromMilliseconds(400));
        await client.ConnectAsync();
        var act = () => client.RequestAsync<JournalQueryRecentParams, JournalQueryRecentResult>(Methods.JournalQueryRecent, new(7));
        await act.Should().ThrowAsync<LotusUnavailableException>();

        await using var garbage = new FakeLotus { Capabilities = Full, SendGarbage = true };
        garbage.Start();
        await using var other = Client(new FixedDiscovery(garbage.Endpoint), timeout: TimeSpan.FromMilliseconds(800));
        (await other.ConnectAsync()).State.Should().Be(LotusConnectionState.Unavailable);
    }

    [Fact]
    public async Task Events_arrive_and_the_client_reconnects_after_lotus_restarts()
    {
        await using var lotus = new FakeLotus { Capabilities = Full };
        lotus.Start();
        await using var client = Client(new FixedDiscovery(lotus.Endpoint));
        var events = new List<string>();
        client.EventReceived += m => { lock (events) events.Add(m.Method!); };
        await client.ConnectAsync();

        await lotus.SendEventAsync(Events.DriverChanged, new DriverChangedEvent("GPU", "560.1", DateTimeOffset.UtcNow));
        await WaitUntil(() => events.Contains(Events.DriverChanged));

        lotus.DropConnection();
        await WaitUntil(() => client.Status.State == LotusConnectionState.Unavailable);
        (await client.ConnectAsync()).State.Should().Be(LotusConnectionState.Connected);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 50 && !condition(); i++) await Task.Delay(100);
        condition().Should().BeTrue();
    }
}
