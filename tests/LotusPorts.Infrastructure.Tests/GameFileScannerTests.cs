using System.Text;
using LotusPorts.Core.GameFiles;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.GameFiles;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

public sealed class GameFileScannerTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly string games;
    private readonly GameFileStore store;
    private readonly GameFileScanner scanner;

    public GameFileScannerTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        games = Path.Combine(folder, "my games");
        Directory.CreateDirectory(Path.Combine(games, "n64"));
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        store = new GameFileStore(db, new SqliteSettingsStore(db, TimeProvider.System), TimeProvider.System);
        scanner = new GameFileScanner(store, NullLogger<GameFileScanner>.Instance);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static byte[] Rom()
    {
        var rom = new byte[1 << 16];
        new byte[] { 0x80, 0x37, 0x12, 0x40 }.CopyTo(rom, 0);
        Encoding.ASCII.GetBytes("TEST").CopyTo(rom, 0x20);
        return rom;
    }

    [Fact]
    public async Task Finds_game_files_reads_each_once_and_forgets_deleted_ones()
    {
        File.WriteAllBytes(Path.Combine(games, "n64", "game.z64"), Rom());
        File.WriteAllText(Path.Combine(games, "notes.txt"), "not a game");
        File.WriteAllText(Path.Combine(games, "fake.n64"), "<html>");

        var first = await scanner.ScanAsync([games]);
        first.Files.Should().ContainSingle().Which.Format.Should().Be(GameFileFormat.N64);
        first.Read.Should().Be(2);

        var second = await scanner.ScanAsync([games]);
        second.Read.Should().Be(0, "same size and date: taken from the cache");
        second.FromCache.Should().Be(2);

        File.Delete(Path.Combine(games, "n64", "game.z64"));
        (await scanner.ScanAsync([games])).Files.Should().BeEmpty();
        (await store.ListAsync()).Should().NotContain(f => f.Path.EndsWith("game.z64", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Watched_folders_are_kept_without_duplicates()
    {
        await store.SetFoldersAsync([games, games.ToUpperInvariant(), "relative\\path"]);
        (await store.GetFoldersAsync()).Should().Equal(games);
    }

    [Fact]
    public async Task Scanning_can_be_cancelled()
    {
        File.WriteAllBytes(Path.Combine(games, "game.z64"), Rom());
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        var act = () => scanner.ScanAsync([games], cancellationToken: cancel.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Games_inside_a_zip_are_read_without_extracting_and_cached_by_the_zips_date()
    {
        var zip = Path.Combine(games, "n64", "Test (USA).zip");
        using (var archive = System.IO.Compression.ZipFile.Open(zip, System.IO.Compression.ZipArchiveMode.Create))
        {
            using (var s = archive.CreateEntry("Test (USA).z64").Open()) s.Write(Rom());
            using (var s = archive.CreateEntry("readme.txt").Open()) s.Write("hi"u8);
        }
        var plain = Path.Combine(folder, "plain.z64");
        File.WriteAllBytes(plain, Rom());
        var expected = (await GameFileReader.ReadAsync(plain)).Hashes;

        var first = await scanner.ScanAsync([games]);
        var entry = first.Files.Should().ContainSingle().Which;
        entry.Path.Should().Be(GameFileZip.EntryPath(zip, "Test (USA).z64"));
        entry.Format.Should().Be(GameFileFormat.N64);
        entry.Hashes.Should().Be(expected, "the same hashes as the plain file");
        Directory.EnumerateFiles(Path.Combine(games, "n64")).Should().ContainSingle("nothing was extracted");

        var second = await scanner.ScanAsync([games]);
        second.FromCache.Should().Be(1);
        second.Read.Should().Be(0);

        File.Delete(zip);
        (await scanner.ScanAsync([games])).Files.Should().BeEmpty();
        (await store.ListAsync()).Should().NotContain(f => f.Path.StartsWith(zip, StringComparison.OrdinalIgnoreCase), "a deleted .zip leaves the list");
    }
}
