using System.Globalization;
using Dapper;

namespace LotusPorts.Infrastructure.Database;

public enum SourceValidationState
{
    Ok,
    Warning,
    NotVerified,
}

public sealed record CatalogSourceRecord(
    string Id,
    string Url,
    string Name,
    bool Active,
    bool Trusted,
    string? ETag,
    DateTimeOffset? LastUpdatedAt,
    SourceValidationState ValidationState,
    string? ValidationMessage,
    DateTimeOffset AddedAt);

public sealed class SqliteCatalogSourceStore(SqliteDatabase database)
{
    public async Task<IReadOnlyList<CatalogSourceRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await c.QueryAsync<Row>(new CommandDefinition("SELECT * FROM CatalogSources ORDER BY AddedAt", cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rows.Select(r => new CatalogSourceRecord(
            r.Id, r.Url, r.Name, r.Active != 0, r.Trusted != 0, r.ETag,
            r.LastUpdatedAt is null ? null : DateTimeOffset.Parse(r.LastUpdatedAt, CultureInfo.InvariantCulture),
            Enum.Parse<SourceValidationState>(r.ValidationState), r.ValidationMessage,
            DateTimeOffset.Parse(r.AddedAt, CultureInfo.InvariantCulture))).ToList();
    }

    public async Task UpsertAsync(CatalogSourceRecord s, CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await c.ExecuteAsync(new CommandDefinition("""
            INSERT INTO CatalogSources (Id, Url, Name, Active, Trusted, ETag, LastUpdatedAt, ValidationState, ValidationMessage, AddedAt)
            VALUES (@Id, @Url, @Name, @Active, @Trusted, @ETag, @LastUpdatedAt, @ValidationState, @ValidationMessage, @AddedAt)
            ON CONFLICT(Id) DO UPDATE SET Url = excluded.Url, Name = excluded.Name, Active = excluded.Active, Trusted = excluded.Trusted,
              ETag = excluded.ETag, LastUpdatedAt = excluded.LastUpdatedAt, ValidationState = excluded.ValidationState, ValidationMessage = excluded.ValidationMessage
            """,
            new
            {
                s.Id,
                s.Url,
                s.Name,
                Active = s.Active ? 1 : 0,
                Trusted = s.Trusted ? 1 : 0,
                s.ETag,
                LastUpdatedAt = s.LastUpdatedAt?.ToString("O", CultureInfo.InvariantCulture),
                ValidationState = s.ValidationState.ToString(),
                s.ValidationMessage,
                AddedAt = s.AddedAt.ToString("O", CultureInfo.InvariantCulture),
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await c.ExecuteAsync(new CommandDefinition("DELETE FROM CatalogSources WHERE Id = @id", new { id }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    private sealed class Row
    {
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public string Name { get; set; } = "";
        public long Active { get; set; }
        public long Trusted { get; set; }
        public string? ETag { get; set; }
        public string? LastUpdatedAt { get; set; }
        public string ValidationState { get; set; } = "";
        public string? ValidationMessage { get; set; }
        public string AddedAt { get; set; } = "";
    }
}
