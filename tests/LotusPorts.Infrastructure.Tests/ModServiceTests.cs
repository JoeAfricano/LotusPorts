using System.Security.Cryptography;
using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Install;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>Phase 8: install, disable, enable and remove mods with backups; never deleting, never touching the picked file.</summary>
public sealed class ModServiceTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly PortLibrary library;
    private readonly SqliteOperationJournal journal;
    private readonly ModService mods;
    private readonly InstalledPort manifest;

    public ModServiceTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        journal = new SqliteOperationJournal(db, TimeProvider.System);
        library = new PortLibrary(paths);
        mods = new ModService(library, journal, TimeProvider.System);
        manifest = new InstalledPort(InstalledPort.CurrentSchema, "mod-port", "v1", null,
            [new InstalledVersion("v1", "v1", "x.zip", 1, null, Core.Verification.CheckState.NotVerified, "Game.exe", DateTimeOffset.UtcNow)], false, false);
        library.Write(manifest);
        Directory.CreateDirectory(library.VersionFolder("mod-port", "v1"));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static Port SamplePort(PortMods? declared) => new(
        1, "mod-port", "mod-game", "Mod", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("o", "r"), null, new WindowsAsset("x", null, false, "zip"), "Game.exe", [],
        new PortSetup(null, null, null, null, null, null, null), [], [],
        new ExperiencePresets(null, null, new Preset(new Dictionary<string, JsonElement>(), false), new Preset(new Dictionary<string, JsonElement>(), false)),
        null, null, null, null, new PortLinks("https://github.com/o/r", null, null), null, null, null, declared);

    private static readonly PortMods Declared = new("mods/", "install", [".otr", ".o2r"], null, "https://github.com/o/r#mods", null);

    private string Download(string name, string content)
    {
        var downloads = Path.Combine(folder, "Downloads");
        Directory.CreateDirectory(downloads);
        var file = Path.Combine(downloads, name);
        File.WriteAllText(file, content);
        return file;
    }

    [Fact]
    public async Task Install_disable_enable_and_remove_keep_everything_recoverable()
    {
        var port = SamplePort(Declared);
        var picked = Download("hd-textures.otr", "v1");
        var hashBefore = SHA256.HashData(File.ReadAllBytes(picked));

        await mods.InstallAsync(port, manifest, picked);
        var gameMods = mods.ModsFolder(port, manifest)!;
        File.ReadAllText(Path.Combine(gameMods, "hd-textures.otr")).Should().Be("v1");
        SHA256.HashData(File.ReadAllBytes(picked)).Should().Equal(hashBefore, "the picked file is only read");

        // Same name again: the old one goes to a backup, never overwritten.
        await mods.InstallAsync(port, manifest, Download("hd-textures.otr", "v2"));
        File.ReadAllText(Path.Combine(gameMods, "hd-textures.otr")).Should().Be("v2");
        Directory.EnumerateFiles(mods.BackupRoot(port), "*", SearchOption.AllDirectories).Select(File.ReadAllText).Should().Contain("v1");

        await mods.DisableAsync(port, manifest, "hd-textures.otr");
        File.Exists(Path.Combine(gameMods, "hd-textures.otr")).Should().BeFalse("the game no longer reads it");
        mods.List(port, manifest).Should().ContainSingle().Which.State.Should().Be(ModState.Disabled);

        await mods.EnableAsync(port, manifest, "hd-textures.otr");
        mods.List(port, manifest).Should().ContainSingle().Which.State.Should().Be(ModState.Enabled);

        var backup = await mods.RemoveAsync(port, manifest, "hd-textures.otr");
        mods.List(port, manifest).Should().BeEmpty();
        File.ReadAllText(backup).Should().Be("v2", "removing moves to a backup, nothing is deleted");

        (await journal.ListRecentAsync(10)).Select(e => e.Kind).Reverse().Should().Equal(
            OperationKind.ModInstall, OperationKind.ModInstall, OperationKind.ModDisable, OperationKind.ModEnable, OperationKind.ModRemove);
    }

    [Fact]
    public async Task Other_file_types_names_with_folders_and_ports_without_mods_are_refused()
    {
        var port = SamplePort(Declared);
        var wrong = async () => await mods.InstallAsync(port, manifest, Download("game.iso", "x"));
        (await wrong.Should().ThrowAsync<ModException>()).Which.Problem.Should().Be(ModProblem.WrongType);

        var sneaky = async () => await mods.RemoveAsync(port, manifest, @"..\..\active.json");
        (await sneaky.Should().ThrowAsync<ModException>()).Which.Problem.Should().Be(ModProblem.NotFound);
        File.Exists(Path.Combine(library.PortFolder("mod-port"), "active.json")).Should().BeTrue();

        ModService.Offers(SamplePort(null)).Should().BeFalse();
        var none = async () => await mods.InstallAsync(SamplePort(null), manifest, Download("a.otr", "x"));
        (await none.Should().ThrowAsync<ModException>()).Which.Problem.Should().Be(ModProblem.NotOffered);
    }
}
