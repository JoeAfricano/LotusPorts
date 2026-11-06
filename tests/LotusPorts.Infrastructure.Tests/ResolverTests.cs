using System.IO.Compression;
using System.Net;
using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Diagnostics;
using LotusPorts.Core.Install;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Styles;
using LotusPorts.Core.Verification;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Diagnostics;
using LotusPorts.Infrastructure.Install;
using LotusPorts.Infrastructure.Network;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>A simulated crash (a real program that exits with an error at once) must lead to "Resolver" with the cause.</summary>
public sealed class ResolverTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly PortLibrary library;
    private readonly PortInstaller installer;
    private readonly PortDiagnostics diagnostics;
    private readonly Downloads server = new();

    public ResolverTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        library = new PortLibrary(paths);
        installer = new PortInstaller(library, paths, new GitHubClient(new HttpClient(server)), new SqliteOperationJournal(db, TimeProvider.System), TimeProvider.System, NullLogger<PortInstaller>.Instance);
        diagnostics = new PortDiagnostics(library, new SystemFolders([], []), TimeProvider.System) { ReadCrashes = (_, _) => [] };
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    private static Port SamplePort() => new(
        1, "crash-port", "crash-game", "Crash", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("owner", "repo"), null, new WindowsAsset("(?i)windows\\.zip$", null, false, "zip"), "Game.exe", [],
        new PortSetup("select-on-first-launch", null, null, null, null, null, null), [], [],
        new ExperiencePresets(null, null, new Preset(new Dictionary<string, JsonElement>(), false), new Preset(new Dictionary<string, JsonElement>(), false)),
        null, null, null, null, new PortLinks("https://github.com/owner/repo", null, null), null, null, null);

    private static LibraryGame Game(Port port)
    {
        var game = new Game(1, port.GameId, "Crash Game", Platform.PC, null, [], "Teste.", port.Id, null, [], "port:" + port.GameId, new Review(ReviewState.Approved, null, null, null));
        return new LibraryGame(game, CatalogOrigin.Official, "Oficial", true, GameVisibility.Public, new PortChoice(port, CatalogOrigin.Official, "Oficial", true, null), []);
    }

    /// <summary>"where.exe" with no arguments prints an error and exits with code 2 right away: a real quick crash.</summary>
    private async Task<InstalledPort> InstallCrashingGameAsync(string tag)
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        await using (var entry = zip.CreateEntry("Game.exe").Open())
            await entry.WriteAsync(await File.ReadAllBytesAsync(Path.Combine(Environment.SystemDirectory, "where.exe")));
        var bytes = memory.ToArray();
        var url = new Uri($"https://github.com/owner/repo/releases/download/{tag}/Game-windows.zip");
        server.Serve(url, bytes);
        var asset = new ReleaseAssetInfo("Game-windows.zip", bytes.Length, null, url);
        return await installer.InstallAsync(SamplePort(), new ReleaseChoice(new ReleaseInfo(tag, tag, false, DateTimeOffset.UtcNow, [asset]), asset));
    }

    private async Task<InstalledPort> RunAsync(Port port)
    {
        var manifest = library.Read(port.Id)!;
        var result = new GameLauncher(library).Start(port, manifest);
        result.Problem.Should().Be(LaunchProblem.None);
        using var process = result.Process!;
        var started = DateTime.UtcNow;
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(20));
        return installer.RecordLaunch(port.Id, process.ExitCode, DateTime.UtcNow - started)!;
    }

    [Fact]
    public async Task A_simulated_crash_turns_the_button_into_resolver_with_the_cause()
    {
        var port = SamplePort();
        await InstallCrashingGameAsync("v1.0");

        var after = await RunAsync(port);

        after.LastLaunchFailed.Should().BeTrue();
        after.LastRun!.ExitCode.Should().NotBe(0);
        PlayButtonRules.Decide(new PlayContext(false, true, true, false, false, false, false, after.LastLaunchFailed, after.CanRollback)).State
            .Should().Be(PlayButtonState.Resolve);

        var report = diagnostics.Run(Game(port), port, after, GameFileState.NotSearched);
        report.MainProblem.Should().Match<ResolveCheck>(c => c.Kind == ResolveCheckKind.LastCrash && c.Reason == "ClosedWithError" && c.Facts[0] == after.LastRun.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
        report.Checks.Single(c => c.Kind == ResolveCheckKind.Program).State.Should().Be(CheckState.Ok);
    }

    [Fact]
    public async Task A_crash_right_after_an_update_puts_rollback_first()
    {
        var port = SamplePort();
        await InstallCrashingGameAsync("v1.0");
        await InstallCrashingGameAsync("v2.0");

        var after = await RunAsync(port);

        after.LastRun!.FirstAfterUpdate.Should().BeTrue();
        var report = diagnostics.Run(Game(port), port, after, GameFileState.Recognized);
        var update = report.Checks.Single(c => c.Kind == ResolveCheckKind.RecentUpdate);
        update.Should().Match<ResolveCheck>(c => c.State == CheckState.Failure && c.Action == ResolveAction.Rollback && c.Facts[1] == "v1.0");
        report.MainProblem!.Kind.Should().Be(ResolveCheckKind.RecentUpdate);
    }

    [Fact]
    public async Task Nothing_is_ok_without_a_real_check()
    {
        var port = SamplePort();
        await InstallCrashingGameAsync("v1.0");
        var report = diagnostics.Run(Game(port), port, library.Read(port.Id), GameFileState.NotSearched);

        // No dependencies, graphics or saves declared, never run: these can only be NOT VERIFIED.
        foreach (var kind in new[] { ResolveCheckKind.GameFile, ResolveCheckKind.Dependencies, ResolveCheckKind.Graphics, ResolveCheckKind.Saves, ResolveCheckKind.LastCrash })
            report.Checks.Single(c => c.Kind == kind).State.Should().Be(CheckState.NotVerified, kind.ToString());
    }

    [Fact]
    public async Task A_missing_program_is_a_failure_with_reinstall()
    {
        var port = SamplePort();
        var manifest = await InstallCrashingGameAsync("v1.0");
        File.Delete(Path.Combine(library.VersionFolder(port.Id, manifest.Active.Folder), "Game.exe"));
        var check = diagnostics.Run(Game(port), port, manifest, GameFileState.Recognized).Checks.Single(c => c.Kind == ResolveCheckKind.Program);
        check.Should().Match<ResolveCheck>(c => c.State == CheckState.Failure && c.Action == ResolveAction.Reinstall);
    }

    [Fact]
    public void Windows_crash_records_are_read_for_the_game_program_only()
    {
        const string xml = """
            <Event xmlns='http://schemas.microsoft.com/win/2004/08/events/event'><System><TimeCreated SystemTime='2026-11-06T09:00:00.000Z'/></System>
            <EventData><Data>Game.exe</Data><Data>1.0</Data><Data>0</Data><Data>vulkan-1.dll</Data><Data>1.3</Data><Data>0</Data><Data>c0000005</Data></EventData></Event>
            <Event xmlns='http://schemas.microsoft.com/win/2004/08/events/event'><System><TimeCreated SystemTime='2026-11-06T10:00:00.000Z'/></System>
            <EventData><Data>Other.exe</Data><Data>1.0</Data><Data>0</Data><Data>x.dll</Data><Data>1</Data><Data>0</Data><Data>c0000409</Data></EventData></Event>
            """;
        var crashes = PortDiagnostics.ParseApplicationErrors(xml, "game.exe");
        crashes.Should().ContainSingle().Which.Should().Match<CrashRecord>(c => c.FaultingModule == "vulkan-1.dll" && c.ExceptionCode == "0xc0000005");
    }

    private sealed class Downloads : HttpMessageHandler
    {
        private readonly Dictionary<Uri, byte[]> files = [];

        public void Serve(Uri url, byte[] bytes) => files[url] = bytes;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(files.TryGetValue(request.RequestUri!, out var bytes)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }
                : new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

/// <summary>Phase 4 acceptance: a style is applied and reverted without losing other settings, and journaled.</summary>
public sealed class StyleServiceTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly PortLibrary library;
    private readonly SqliteOperationJournal journal;

    public StyleServiceTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        journal = new SqliteOperationJournal(db, TimeProvider.System);
        library = new PortLibrary(paths);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static Port Port(bool verified) => new(
        1, "style-port", "style-game", "Style", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("o", "r"), null, new WindowsAsset("x", null, false, "zip"), "Game.exe", [],
        new PortSetup(null, null, null, null, null, null, null), [], [],
        new ExperiencePresets("config/settings.json", "json",
            new Preset(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{ "video.scale": 1, "video.widescreen": false }""")!, verified),
            new Preset(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{ "video.scale": 4, "video.widescreen": true }""")!, verified)),
        null, null, null, null, new PortLinks("https://github.com/o/r", null, null), null, null, null);

    private string InstallFake()
    {
        var version = new InstalledVersion("v1", "v1", "x.zip", 1, null, Core.Verification.CheckState.NotVerified, "Game.exe", DateTimeOffset.UtcNow);
        library.Write(new InstalledPort(InstalledPort.CurrentSchema, "style-port", "v1", null, [version], false, false));
        var config = Path.Combine(library.VersionFolder("style-port", "v1"), "config", "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(config)!);
        File.WriteAllText(config, """{ "video": { "scale": 2, "vsync": true }, "input": { "deadzone": 0.2 } }""");
        return config;
    }

    [Fact]
    public async Task Remastered_is_applied_and_reverted_keeping_the_players_other_settings()
    {
        var config = InstallFake();
        var service = new StyleService(library, journal, TimeProvider.System);

        var applied = await service.ApplyAsync(Port(verified: true), ExperienceStyle.Remastered);
        applied.Style!.Name.Should().Be("Remastered");
        File.Exists(applied.Style.Backup).Should().BeTrue();
        var text = File.ReadAllText(config);
        PresetWriter.Read(text, "json", ["video.scale", "video.widescreen", "video.vsync", "input.deadzone"]).Values
            .Should().Equal("4", "true", "true", "0.2");

        var reverted = await service.RevertAsync(Port(verified: true));
        reverted.Style.Should().BeNull();
        var back = PresetWriter.Read(File.ReadAllText(config), "json", ["video.scale", "video.widescreen", "video.vsync", "input.deadzone"]);
        back["video.scale"].Should().Be("2");
        back["video.widescreen"].Should().BeNull();
        back["video.vsync"].Should().Be("true");
        back["input.deadzone"].Should().Be("0.2");

        (await journal.ListRecentAsync(5)).Select(e => (e.Kind, e.Outcome)).Should().Equal(
            (OperationKind.RevertPreset, OperationOutcome.Success), (OperationKind.ApplyPreset, OperationOutcome.Success));
    }

    [Fact]
    public async Task Without_verified_keys_nothing_is_written()
    {
        var config = InstallFake();
        var before = File.ReadAllText(config);
        StyleService.CanOffer(Port(verified: false)).Should().BeFalse();
        var act = () => new StyleService(library, journal, TimeProvider.System).ApplyAsync(Port(verified: false), ExperienceStyle.Remastered);
        (await act.Should().ThrowAsync<StyleException>()).Which.Problem.Should().Be(StyleProblem.NotVerified);
        File.ReadAllText(config).Should().Be(before);
    }
}
