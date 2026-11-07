using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.GameFiles;
using LotusPorts.Core.Install;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Install;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>"Colocar na pasta do jogo": the recognized copy goes where the port reads it; the original and an old file are kept.</summary>
public sealed class GameFilePlacerTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly PortLibrary library;
    private readonly GameFilePlacer placer;
    private readonly InstalledPort manifest;

    public GameFilePlacerTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        library = new PortLibrary(paths);
        placer = new GameFilePlacer(library, new SqliteOperationJournal(db, TimeProvider.System), TimeProvider.System);
        manifest = new InstalledPort(InstalledPort.CurrentSchema, "pd", "v1", null,
            [new InstalledVersion("v1", "v1", "x.zip", 1, null, Core.Verification.CheckState.NotVerified, "pd.exe", DateTimeOffset.UtcNow)], false, false);
        library.Write(manifest);
        Directory.CreateDirectory(library.VersionFolder("pd", "v1"));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static Port BesideExe(string? folderName, string? fileName, string kind = "single-file") => new(
        1, "pd", "perfect-dark", "PD", "decomp-port", PortStatus.JOGAVEL, null, null, null,
        new RepoRef("o", "r"), null, new WindowsAsset("x", null, false, "zip"), "pd.exe", [],
        new PortSetup("beside-exe", fileName, folderName, null, null, null, null),
        [new RequiredGameFile("Cartucho", kind, "n64-rom", null, [], null)], [],
        new ExperiencePresets(null, null, new Preset(new Dictionary<string, JsonElement>(), false), new Preset(new Dictionary<string, JsonElement>(), false)),
        null, null, null, null, new PortLinks("https://github.com/o/r", null, null), null, null, null);

    private GameFileInfo N64ByteSwapped(string name)
    {
        // .v64 (byte-swapped) signature: 37 80 40 12.
        var rom = new byte[4096];
        rom[0] = 0x37; rom[1] = 0x80; rom[2] = 0x40; rom[3] = 0x12;
        var path = Path.Combine(folder, name);
        File.WriteAllBytes(path, rom);
        return new GameFileInfo(path, rom.Length, DateTime.UtcNow, GameFileFormat.N64, N64ByteOrder.ByteSwapped, null, null);
    }

    [Fact]
    public async Task The_copy_goes_to_the_ports_folder_with_its_name_in_z64_order_and_the_old_file_is_kept()
    {
        var port = BesideExe("data", "pd.ntsc-final.z64");
        GameFilePlacer.Offers(port).Should().BeTrue();
        var source = N64ByteSwapped("Perfect Dark (USA).v64");
        var before = File.ReadAllBytes(source.Path);
        var target = Path.Combine(library.VersionFolder("pd", "v1"), "data", "pd.ntsc-final.z64");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllText(target, "old");

        placer.IsPlaced(port, manifest, source.Path).Should().BeFalse();
        (await placer.PlaceAsync(port, manifest, source)).Should().Be(target);

        var placed = File.ReadAllBytes(target);
        placed[..4].Should().Equal(new byte[] { 0x80, 0x37, 0x12, 0x40 }, "written in .z64 order");
        File.ReadAllBytes(source.Path).Should().Equal(before, "the player's file is only read");
        placer.IsPlaced(port, manifest, source.Path).Should().BeTrue();
        Directory.EnumerateFiles(Path.Combine(library.PortFolder("pd"), "game-file-backups"), "*", SearchOption.AllDirectories)
            .Should().ContainSingle().Which.Should().EndWith("pd.ntsc-final.z64");
    }

    [Fact]
    public void Only_ports_that_read_one_file_from_their_folder_are_offered_and_never_outside_it()
    {
        GameFilePlacer.Offers(BesideExe(null, null, kind: "multiple")).Should().BeFalse();
        var escaping = BesideExe("..\\..", "x.z64");
        var act = () => placer.TargetPath(escaping, manifest, "C:\\x.z64");
        act.Should().Throw<InvalidOperationException>();
        placer.TargetPath(BesideExe(null, null), manifest, "C:\\games\\Zelda.gba").Should().EndWith("\\Zelda.gba");
    }
}
