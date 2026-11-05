using System.Globalization;
using Dapper;

namespace LotusPorts.Infrastructure.Database;

public sealed record StoredGameState(string GameId, DateTimeOffset FirstSeenAt, bool Favorite, bool DismissedFromNew);

/// <summary>What the player did with each game: first seen, favorite, dismissed from "Chegou!".</summary>
public sealed class SqliteGameStateStore(SqliteDatabase database, TimeProvider time)
{
    public async Task<IReadOnlyDictionary<string, StoredGameState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await c.QueryAsync<(string GameId, string FirstSeenAt, long Favorite, long DismissedFromNew)>(
            new CommandDefinition("SELECT GameId, FirstSeenAt, Favorite, DismissedFromNew FROM Games", cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rows.ToDictionary(r => r.GameId, r => new StoredGameState(r.GameId, DateTimeOffset.Parse(r.FirstSeenAt, CultureInfo.InvariantCulture), r.Favorite != 0, r.DismissedFromNew != 0));
    }

    /// <summary>Records games seen for the first time. Returns the moment of the very first catalog load (the "new" baseline).</summary>
    public async Task<DateTimeOffset> MarkSeenAsync(IEnumerable<string> gameIds, CancellationToken cancellationToken = default)
    {
        var now = time.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var tx = await c.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        foreach (var id in gameIds)
            await c.ExecuteAsync("INSERT OR IGNORE INTO Games (GameId, FirstSeenAt) VALUES (@id, @now)", new { id, now }, tx).ConfigureAwait(false);
        var first = await c.ExecuteScalarAsync<string?>("SELECT MIN(FirstSeenAt) FROM Games", transaction: tx).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        return first is null ? time.GetUtcNow() : DateTimeOffset.Parse(first, CultureInfo.InvariantCulture);
    }

    public Task SetFavoriteAsync(string gameId, bool favorite, CancellationToken cancellationToken = default) =>
        Update("Favorite", gameId, favorite, cancellationToken);

    public Task DismissFromNewAsync(string gameId, CancellationToken cancellationToken = default) =>
        Update("DismissedFromNew", gameId, true, cancellationToken);

    private async Task Update(string column, string gameId, bool value, CancellationToken cancellationToken)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var now = time.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);
        // Column names come from this class only, never from input.
        await c.ExecuteAsync(new CommandDefinition(
            $"INSERT INTO Games (GameId, FirstSeenAt, {column}) VALUES (@gameId, @now, @value) ON CONFLICT(GameId) DO UPDATE SET {column} = excluded.{column}",
            new { gameId, now, value = value ? 1 : 0 }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}
