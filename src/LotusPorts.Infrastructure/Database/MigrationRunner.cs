using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Database;

public interface IMigration
{
    int Version { get; }
    string Name { get; }
    string Sql { get; }
}

/// <summary>The database was written by a newer Lotus Ports. It is left untouched.</summary>
public sealed class DatabaseNewerThanAppException(int databaseVersion, int appVersion)
    : Exception($"Database schema v{databaseVersion} is newer than this app (v{appVersion}).")
{
    public int DatabaseVersion { get; } = databaseVersion;
    public int AppVersion { get; } = appVersion;
}

/// <summary>Applies versioned migrations in order, each in its own transaction. Version lives in PRAGMA user_version.</summary>
public sealed class MigrationRunner(SqliteDatabase database, IEnumerable<IMigration> migrations, ILogger<MigrationRunner> logger)
{
    private readonly IReadOnlyList<IMigration> ordered = Validate(migrations);

    public int LatestVersion => ordered.Count == 0 ? 0 : ordered[^1].Version;

    public async Task<int> MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync("PRAGMA journal_mode=WAL;").ConfigureAwait(false);
        var current = await connection.ExecuteScalarAsync<int>("PRAGMA user_version;").ConfigureAwait(false);
        if (current > LatestVersion) throw new DatabaseNewerThanAppException(current, LatestVersion);
        foreach (var migration in ordered.Where(m => m.Version > current))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await connection.ExecuteAsync(migration.Sql, transaction: transaction).ConfigureAwait(false);
            // PRAGMA does not take parameters; the version is an int from code, not user input.
            await connection.ExecuteAsync($"PRAGMA user_version = {migration.Version};", transaction: transaction).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Database migrated to v{Version} ({Name})", migration.Version, migration.Name);
            current = migration.Version;
        }
        return current;
    }

    private static List<IMigration> Validate(IEnumerable<IMigration> migrations)
    {
        var list = migrations.OrderBy(m => m.Version).ToList();
        for (var i = 0; i < list.Count; i++)
            if (list[i].Version != i + 1) throw new InvalidOperationException($"Migrations must be numbered 1..n without gaps; found v{list[i].Version} at position {i + 1}.");
        return list;
    }
}
