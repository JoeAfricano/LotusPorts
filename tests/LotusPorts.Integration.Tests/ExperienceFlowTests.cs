using System.Text.Json;
using Lotus.Integration.Contracts;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Styles;
using LotusPorts.Infrastructure;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Install;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Integration.Tests;

/// <summary>
/// Phase 6 acceptance with a simulated Lotus: a session produces a measured report; Lotus's suggestion (Remastered) is
/// applied through the style flow; the next session brings Lotus's before/after comparison.
/// </summary>
public sealed class ExperienceFlowTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static Port VerifiedPort() => new(
        1, "flow-port", "flow-game", "Flow", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("o", "r"), null, new WindowsAsset("x", null, false, "zip"), "Game.exe", [],
        new PortSetup(null, null, null, null, null, null, null), [], [],
        new ExperiencePresets("settings.json", "json",
            new Preset(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{ "scale": 1 }""")!, true),
            new Preset(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{ "scale": 4 }""")!, true)),
        null, null, null, null, new PortLinks("https://github.com/o/r", null, null), null, null, null);

    [Fact]
    public async Task Session_report_suggestion_applied_and_compared()
    {
        // Simulated Lotus: measures sessions and suggests Remastered; once a session ran with it, it compares.
        var sessions = new List<SessionStartParams>();
        await using var lotus = new FakeLotus
        {
            Capabilities = [Methods.SessionStart, Methods.SessionEnd, Methods.SessionGetReport, Methods.RecommendationsForGame, Events.PresetApplied],
        };
        lotus.Handlers[Methods.SessionStart] = p =>
        {
            var start = p!.Value.Deserialize<SessionStartParams>(ContractJson.Options)!;
            lock (sessions) sessions.Add(start);
            return new SessionStartResult("s" + sessions.Count);
        };
        lotus.Handlers[Methods.SessionEnd] = _ => new { ok = true };
        lotus.Handlers[Methods.SessionGetReport] = p => new SessionReport(p!.Value.GetProperty("sessionId").GetString()!, true, 1800, 58.5, 41, 3, null, 71);
        lotus.Handlers[Methods.RecommendationsForGame] = _ =>
        {
            var withStyle = sessions.LastOrDefault()?.Preset == "Remastered";
            return new RecommendationsResult([new Recommendation("r1", "Usar o estilo Remasterizado neste PC", "3 sessões com GPU folgada", "Remastered",
                withStyle ? "1% low de 41 para 55 FPS" : null)]);
        };
        lotus.Start();
        await using var client = new LotusClient(new FixedDiscovery(lotus.Endpoint), new FixedValidator(true), NullLogger<LotusClient>.Instance, "0.1.0");
        (await client.ConnectAsync()).State.Should().Be(LotusConnectionState.Connected);

        // An installed game with verified styles (temp library, nothing on the real PC).
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        await new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync();
        var library = new PortLibrary(paths);
        library.Write(new InstalledPort(InstalledPort.CurrentSchema, "flow-port", "v1",
            null, [new InstalledVersion("v1", "v1", "x.zip", 1, null, Core.Verification.CheckState.NotVerified, "Game.exe", DateTimeOffset.UtcNow)], false, false));
        var config = Path.Combine(library.VersionFolder("flow-port", "v1"), "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(config)!);
        File.WriteAllText(config, """{ "scale": 2, "volume": 80 }""");
        var styles = new StyleService(library, new SqliteOperationJournal(db, TimeProvider.System), TimeProvider.System);

        // 1st session → measured report.
        var first = await client.RequestAsync<SessionStartParams, SessionStartResult>(Methods.SessionStart, new("port:flow-game", "Flow", "flow-port", "v1", config, 1, null));
        await client.RequestAsync<SessionEndParams, object>(Methods.SessionEnd, new(first!.SessionId, 0, 1800));
        var report = await client.RequestAsync<SessionGetReportParams, SessionReport>(Methods.SessionGetReport, new(first.SessionId));
        report!.Should().Match<SessionReport>(r => r.Measured && r.OnePercentLowFps == 41 && r.Stutters == 3);

        // Suggestion → applied through the style flow (backup, only declared keys, journal), and Lotus is told.
        var suggestion = (await client.RequestAsync<RecommendationsForGameParams, RecommendationsResult>(Methods.RecommendationsForGame, new("port:flow-game", [first.SessionId])))!.Items.Single();
        suggestion.ComparisonPtBr.Should().BeNull("no session ran with the suggestion yet");
        var applied = await styles.ApplyAsync(VerifiedPort(), Enum.Parse<ExperienceStyle>(suggestion.Style!));
        await client.NotifyAsync(Events.PresetApplied, new GameEvent("flow-game", "flow-port", "v1"));
        PresetWriter.Read(File.ReadAllText(config), "json", ["scale", "volume"]).Values.Should().Equal("4", "80");

        // 2nd session with the style → Lotus compares before/after.
        var second = await client.RequestAsync<SessionStartParams, SessionStartResult>(Methods.SessionStart, new("port:flow-game", "Flow", "flow-port", "v1", config, 2, applied.Style!.Name));
        await client.RequestAsync<SessionEndParams, object>(Methods.SessionEnd, new(second!.SessionId, 0, 1200));
        var compared = (await client.RequestAsync<RecommendationsForGameParams, RecommendationsResult>(Methods.RecommendationsForGame, new("port:flow-game", [second.SessionId, first.SessionId])))!.Items.Single();
        compared.ComparisonPtBr.Should().Be("1% low de 41 para 55 FPS");

        await Task.Delay(200);
        lotus.Received.Select(m => m.Method).Should().Contain(Events.PresetApplied);
    }
}
