using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Verification;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Install;
using LotusPorts.Infrastructure.Network;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>Install → update → roll back → uninstall against fake GitHub downloads (no network, temp folders only).</summary>
public sealed class PortInstallerTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly Downloads server = new();
    private readonly AppPaths paths;
    private readonly PortLibrary library;
    private readonly SqliteOperationJournal journal;
    private readonly PortInstaller installer;

    public PortInstallerTests()
    {
        paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        journal = new SqliteOperationJournal(db, TimeProvider.System);
        library = new PortLibrary(paths);
        installer = new PortInstaller(library, paths, new GitHubClient(new HttpClient(server)), journal, TimeProvider.System, NullLogger<PortInstaller>.Instance);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static Port SamplePort(string package = "zip", PortEvidence? evidence = null) => new(
        1, "sample-port", "sample-game", "Sample", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("owner", "repo"), null, new WindowsAsset("(?i)windows\\.zip$", null, false, package), "Game.exe", ["--fullscreen"],
        new PortSetup(null, null, null, null, null, null, null), [],
        [new PersistentPath("saves/", "saves", "install"), new PersistentPath("game.ini", "config", "install"), new PersistentPath("%APPDATA%\\Sample", "saves", "user")],
        new ExperiencePresets(null, null, new Preset(new Dictionary<string, JsonElement>(), false), new Preset(new Dictionary<string, JsonElement>(), false)),
        null, null, null, null, new PortLinks("https://github.com/owner/repo", null, null), null, null, evidence);

    private static byte[] Zip(params (string Name, string Content)[] files)
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, content) in files)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open());
                writer.Write(content);
            }
        return memory.ToArray();
    }

    private const string Program = "MZ fake program";

    private ReleaseChoice Release(string tag, byte[] bytes, bool publishDigest = true)
    {
        var url = new Uri($"https://github.com/owner/repo/releases/download/{tag}/Game-{tag}-Windows.zip");
        server.Serve(url, bytes);
        var digest = publishDigest ? Convert.ToHexStringLower(SHA256.HashData(bytes)) : null;
        var asset = new ReleaseAssetInfo($"Game-{tag}-Windows.zip", bytes.Length, digest, url);
        return new ReleaseChoice(new ReleaseInfo(tag, tag, false, DateTimeOffset.UtcNow, [asset]), asset);
    }

    private string Active(InstalledPort m) => library.VersionFolder("sample-port", m.Active.Folder);

    [Fact]
    public async Task Full_cycle_keeps_saves_and_can_go_back()
    {
        var port = SamplePort();
        var stages = new List<InstallStage>();

        // Install: top folder flattened, program found, digest checked.
        var v1 = await installer.InstallAsync(port, Release("v1.0", Zip(("Game-v1.0/Game.exe", Program), ("Game-v1.0/game.ini", "default"))),
            new SyncProgress<InstallProgress>(p => stages.Add(p.Stage)));
        v1.ActiveTag.Should().Be("v1.0");
        v1.Active.Integrity.Should().Be(CheckState.Ok);
        v1.Active.Executable.Should().Be("Game.exe");
        v1.PendingFirstLaunch.Should().BeFalse();
        stages.Should().ContainInOrder(InstallStage.Downloading, InstallStage.Extracting, InstallStage.Finishing);

        // The player plays: saves and settings written inside the game folder.
        Directory.CreateDirectory(Path.Combine(Active(v1), "saves"));
        File.WriteAllText(Path.Combine(Active(v1), "saves", "slot1.sav"), "progress v1");
        File.WriteAllText(Path.Combine(Active(v1), "game.ini"), "my settings");

        // Update: new folder, saves and settings carried over, old version kept untouched.
        var v2 = await installer.InstallAsync(port, Release("v2.0", Zip(("Game.exe", Program), ("game.ini", "new default"))));
        v2.ActiveTag.Should().Be("v2.0");
        v2.PreviousTag.Should().Be("v1.0");
        v2.PendingFirstLaunch.Should().BeTrue();
        File.ReadAllText(Path.Combine(Active(v2), "saves", "slot1.sav")).Should().Be("progress v1");
        File.ReadAllText(Path.Combine(Active(v2), "game.ini")).Should().Be("my settings");
        Directory.Exists(Active(v1)).Should().BeTrue();

        // The first launch after the update crashed: the button offers rollback.
        installer.RecordLaunch("sample-port", exitCode: -1073741819, TimeSpan.FromSeconds(3))!.Should().Match<InstalledPort>(m => m.LastLaunchFailed && m.LastRun!.FirstAfterUpdate && m.LastRun.Tag == "v2.0");
        PlayButtonRules.Decide(new PlayContext(false, true, true, false, false, false, false, true, library.Read("sample-port")!.CanRollback))
            .Secondary.Should().StartWith(SecondaryAction.Rollback);

        // Roll back: v1 active again with its saves as they were before the update.
        File.WriteAllText(Path.Combine(Active(v2), "saves", "slot1.sav"), "progress v2");
        var back = await installer.RollbackAsync(port);
        back.ActiveTag.Should().Be("v1.0");
        back.PreviousTag.Should().Be("v2.0");
        back.LastLaunchFailed.Should().BeFalse();
        File.ReadAllText(Path.Combine(Active(back), "saves", "slot1.sav")).Should().Be("progress v1");

        // A third version prunes the oldest one not in use (keeps active + previous).
        var v3 = await installer.InstallAsync(port, Release("v3.0", Zip(("Game.exe", Program))));
        v3.Versions.Select(v => v.Tag).Should().BeEquivalentTo(["v1.0", "v3.0"]);
        Directory.Exists(library.VersionFolder("sample-port", "v2.0")).Should().BeFalse();

        // Uninstall keeping saves, then install again: saves come back.
        await installer.RemoveAsync(port, keepSaves: true);
        library.Read("sample-port").Should().BeNull();
        var again = await installer.InstallAsync(port, Release("v3.0", Zip(("Game.exe", Program))));
        File.ReadAllText(Path.Combine(Active(again), "saves", "slot1.sav")).Should().Be("progress v1");
        Directory.Exists(library.KeptFolder("sample-port")).Should().BeFalse();

        Directory.Exists(Path.Combine(paths.DownloadsCache, "sample-port")).Should().BeFalse("no empty download folder is left behind");

        // Remove everything: the port folder is gone.
        await installer.RemoveAsync(port, keepSaves: false);
        Directory.Exists(Path.Combine(library.Root, "sample-port")).Should().BeFalse();

        var entries = await journal.ListRecentAsync(20);
        entries.Select(e => e.Kind).Reverse().Should().Equal(
            OperationKind.Install, OperationKind.Update, OperationKind.Rollback, OperationKind.Update, OperationKind.Remove, OperationKind.Install, OperationKind.Remove);
        entries.Should().OnlyContain(e => e.Outcome == OperationOutcome.Success && e.StateAfter != null);
    }

    [Fact]
    public async Task Uninstalling_with_nothing_to_keep_leaves_no_empty_folder()
    {
        var port = SamplePort() with { PersistentPaths = [] };
        await installer.InstallAsync(port, Release("v1.0", Zip(("Game.exe", Program))));
        await installer.RemoveAsync(port, keepSaves: true);
        Directory.Exists(Path.Combine(library.Root, "sample-port")).Should().BeFalse();
    }

    [Fact]
    public async Task A_file_that_does_not_match_the_published_digest_is_refused()
    {
        var choice = Release("v1.0", Zip(("Game.exe", Program)));
        var wrong = choice with { Asset = choice.Asset with { Sha256 = new string('0', 64) } };

        var act = () => installer.InstallAsync(SamplePort(), wrong);

        (await act.Should().ThrowAsync<InstallException>()).Which.Failure.Should().Be(InstallFailure.IntegrityMismatch);
        library.Read("sample-port").Should().BeNull();
        Directory.EnumerateFiles(paths.DownloadsCache, "*", SearchOption.AllDirectories).Should().BeEmpty();
        (await journal.ListRecentAsync(1)).Single().Outcome.Should().Be(OperationOutcome.Failure);
    }

    [Fact]
    public async Task Without_a_published_digest_the_version_is_not_verified_unless_the_catalog_recorded_it()
    {
        var bytes = Zip(("Game.exe", Program));
        var plain = await installer.InstallAsync(SamplePort(), Release("v1.0", bytes, publishDigest: false));
        plain.Active.Integrity.Should().Be(CheckState.NotVerified);

        var choice = Release("v2.0", bytes, publishDigest: false);
        var evidence = new PortEvidence(null, "v2.0", null, null, false, choice.Asset.Name, choice.Asset.Size, Convert.ToHexStringLower(SHA256.HashData(bytes)));
        (await installer.InstallAsync(SamplePort(evidence: evidence), choice)).Active.Integrity.Should().Be(CheckState.Ok);
    }

    [Fact]
    public async Task A_version_without_the_program_is_not_installed()
    {
        var act = () => installer.InstallAsync(SamplePort(), Release("v1.0", Zip(("readme.txt", "hello"))));
        (await act.Should().ThrowAsync<InstallException>()).Which.Failure.Should().Be(InstallFailure.NoExecutable);
        Directory.EnumerateDirectories(library.VersionsFolder("sample-port")).Should().BeEmpty();
    }

    [Fact]
    public async Task Cancelling_leaves_nothing_behind()
    {
        using var cancel = new CancellationTokenSource();
        var big = Zip(("Game.exe", Program + new string(' ', 300_000)));
        var act = () => installer.InstallAsync(SamplePort(), Release("v1.0", big),
            new SyncProgress<InstallProgress>(p => { if (p.Stage == InstallStage.Downloading) cancel.Cancel(); }), cancel.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        library.Read("sample-port").Should().BeNull();
        Directory.EnumerateFiles(paths.DownloadsCache, "*", SearchOption.AllDirectories).Should().BeEmpty();
        (await journal.ListRecentAsync(1)).Single().Outcome.Should().Be(OperationOutcome.Cancelled);
    }

    [Fact]
    public async Task Installers_and_unknown_packages_are_left_to_the_official_page()
    {
        var act = () => installer.InstallAsync(SamplePort("installer"), Release("v1.0", Zip(("Game.exe", Program))));
        (await act.Should().ThrowAsync<InstallException>()).Which.Failure.Should().Be(InstallFailure.UnsupportedPackage);
    }

    [Fact]
    public void Leftovers_of_an_interrupted_install_are_cleaned()
    {
        var staging = Path.Combine(library.VersionsFolder("sample-port"), "v1.0" + PortLibrary.StagingSuffix);
        Directory.CreateDirectory(staging);
        Directory.CreateDirectory(Path.Combine(paths.DownloadsCache, "sample-port"));
        File.WriteAllText(Path.Combine(paths.DownloadsCache, "sample-port", "x.zip.part"), "half");

        library.CleanLeftovers().Should().Be(2);
        Directory.Exists(staging).Should().BeFalse();
    }

    /// <summary>Reports on the calling thread, so the test sees every stage in order.</summary>
    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class Downloads : HttpMessageHandler
    {
        private readonly Dictionary<Uri, byte[]> files = [];

        public void Serve(Uri url, byte[] bytes) => files[url] = bytes;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(files.TryGetValue(request.RequestUri!, out var bytes)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }
                : new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
