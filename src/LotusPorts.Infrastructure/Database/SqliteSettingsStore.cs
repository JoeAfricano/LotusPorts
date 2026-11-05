using System.Globalization;
using Dapper;
using LotusPorts.Core.Preferences;

namespace LotusPorts.Infrastructure.Database;

public sealed class SqliteSettingsStore(SqliteDatabase database, TimeProvider time) : ISettingsStore
{
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await connection.ExecuteScalarAsync<string?>(
            new CommandDefinition("SELECT Value FROM Settings WHERE Key = @key", new { key }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO Settings (Key, Value, UpdatedAt) VALUES (@key, @value, @at)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value, UpdatedAt = excluded.UpdatedAt
            """,
            new { key, value, at = time.GetUtcNow().ToString("O", CultureInfo.InvariantCulture) },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}
