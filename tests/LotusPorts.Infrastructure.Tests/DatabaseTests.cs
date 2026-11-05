using Dapper;
using LotusPorts.Core.Operations;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

public sealed class DatabaseTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly SqliteDatabase database;

    public DatabaseTests()
    {
        Directory.CreateDirectory(folder);
        database = new SqliteDatabase(Path.Combine(folder, "test.db"));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, recursive: true); } catch (IOException) { }
    }

    private MigrationRunner Runner(IEnumerable<IMigration>? migrations = null) =>
        new(database, migrations ?? AllMigrations.List, NullLogger<MigrationRunner>.Instance);

    private async Task<SqliteOperationJournal> MigratedJournal()
    {
        await Runner().MigrateAsync();
        return new SqliteOperationJournal(database, TimeProvider.System);
    }

    [Fact]
    public async Task Fresh_database_is_migrated_with_wal()
    {
        (await Runner().MigrateAsync()).Should().Be(AllMigrations.List.Count);
        await using var c = await database.OpenAsync();
        (await c.ExecuteScalarAsync<string>("PRAGMA journal_mode;")).Should().Be("wal");
        (await c.QueryAsync<string>("SELECT name FROM sqlite_master WHERE type = 'table'"))
            .Should().Contain(["Settings", "OperationJournal", "Games", "CatalogSources"]);
    }

    [Fact]
    public async Task Migrating_twice_changes_nothing()
    {
        await Runner().MigrateAsync();
        (await Runner().MigrateAsync()).Should().Be(AllMigrations.List.Count);
    }

    [Fact]
    public async Task Newer_database_is_left_untouched()
    {
        await using (var c = await database.OpenAsync()) await c.ExecuteAsync("PRAGMA user_version = 7;");
        var act = () => Runner().MigrateAsync();
        (await act.Should().ThrowAsync<DatabaseNewerThanAppException>()).Which.DatabaseVersion.Should().Be(7);
    }

    [Fact]
    public void Migration_numbering_gaps_are_rejected()
    {
        var act = () => Runner([new M001Foundation(), new FakeMigration(3)]);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Settings_upsert_and_read()
    {
        await Runner().MigrateAsync();
        var store = new SqliteSettingsStore(database, TimeProvider.System);
        (await store.GetAsync("k")).Should().BeNull();
        await store.SetAsync("k", "1");
        await store.SetAsync("k", "2");
        (await store.GetAsync("k")).Should().Be("2");
    }

    [Fact]
    public async Task Journal_keeps_before_and_after()
    {
        var journal = await MigratedJournal();
        var correlation = Guid.NewGuid();
        var id = await journal.BeginAsync(OperationKind.Install, "ship-of-harkinian", """{"installed":false}""", correlation);

        (await journal.ListOpenAsync()).Should().ContainSingle(e => e.Id == id && e.IsOpen);

        await journal.CompleteAsync(id, OperationOutcome.Success, """{"installed":true}""", null);

        var entry = (await journal.ListRecentAsync(10)).Single();
        entry.Should().BeEquivalentTo(new
        {
            Kind = OperationKind.Install,
            PortId = "ship-of-harkinian",
            StateBefore = """{"installed":false}""",
            StateAfter = """{"installed":true}""",
            Outcome = (OperationOutcome?)OperationOutcome.Success,
            CorrelationId = correlation,
        });
        entry.FinishedAt.Should().NotBeNull();
        (await journal.ListOpenAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Journal_rejects_invalid_json_and_double_completion()
    {
        var journal = await MigratedJournal();
        var bad = () => journal.BeginAsync(OperationKind.Install, null, "not json", Guid.NewGuid());
        await bad.Should().ThrowAsync<ArgumentException>();

        var id = await journal.BeginAsync(OperationKind.Install, null, "{}", Guid.NewGuid());
        await journal.CompleteAsync(id, OperationOutcome.Failure, null, "boom");
        var again = () => journal.CompleteAsync(id, OperationOutcome.Success, "{}", null);
        await again.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class FakeMigration(int version) : IMigration
    {
        public int Version => version;
        public string Name => "Fake";
        public string Sql => "SELECT 1;";
    }
}
