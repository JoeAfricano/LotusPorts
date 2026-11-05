using System.Globalization;
using System.Text.Json;
using Dapper;
using LotusPorts.Core.Operations;

namespace LotusPorts.Infrastructure.Database;

public sealed class SqliteOperationJournal(SqliteDatabase database, TimeProvider time) : IOperationJournal
{
    private const string Columns = "Id, StartedAt, FinishedAt, Kind, PortId, StateBefore, StateAfter, Outcome, Details, CorrelationId";

    public async Task<long> BeginAsync(OperationKind kind, string? portId, string stateBeforeJson, Guid correlationId, CancellationToken cancellationToken = default)
    {
        EnsureJson(stateBeforeJson, nameof(stateBeforeJson));
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            """
            INSERT INTO OperationJournal (StartedAt, Kind, PortId, StateBefore, CorrelationId)
            VALUES (@at, @kind, @portId, @before, @correlationId);
            SELECT last_insert_rowid();
            """,
            new { at = Now(), kind = kind.ToString(), portId, before = stateBeforeJson, correlationId = correlationId.ToString("D") },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task CompleteAsync(long id, OperationOutcome outcome, string? stateAfterJson, string? details, CancellationToken cancellationToken = default)
    {
        if (stateAfterJson is not null) EnsureJson(stateAfterJson, nameof(stateAfterJson));
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE OperationJournal SET FinishedAt = @at, Outcome = @outcome, StateAfter = @after, Details = @details WHERE Id = @id AND Outcome IS NULL",
            new { id, at = Now(), outcome = outcome.ToString(), after = stateAfterJson, details },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
        if (rows != 1) throw new InvalidOperationException($"Journal entry {id} does not exist or is already complete.");
    }

    public async Task<IReadOnlyList<OperationEntry>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await connection.QueryAsync<Row>(new CommandDefinition(
            $"SELECT {Columns} FROM OperationJournal ORDER BY Id DESC LIMIT @limit", new { limit }, cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rows.Select(ToEntry).ToList();
    }

    public async Task<IReadOnlyList<OperationEntry>> ListOpenAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await connection.QueryAsync<Row>(new CommandDefinition(
            $"SELECT {Columns} FROM OperationJournal WHERE Outcome IS NULL ORDER BY Id", cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rows.Select(ToEntry).ToList();
    }

    private string Now() => time.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);

    private static void EnsureJson(string json, string name)
    {
        try { using var _ = JsonDocument.Parse(json); }
        catch (JsonException ex) { throw new ArgumentException("Journal states must be valid JSON.", name, ex); }
    }

    private static OperationEntry ToEntry(Row r) => new(
        r.Id,
        DateTimeOffset.Parse(r.StartedAt, CultureInfo.InvariantCulture),
        r.FinishedAt is null ? null : DateTimeOffset.Parse(r.FinishedAt, CultureInfo.InvariantCulture),
        Enum.Parse<OperationKind>(r.Kind),
        r.PortId,
        r.StateBefore,
        r.StateAfter,
        r.Outcome is null ? null : Enum.Parse<OperationOutcome>(r.Outcome),
        r.Details,
        Guid.Parse(r.CorrelationId));

    private sealed class Row
    {
        public long Id { get; set; }
        public string StartedAt { get; set; } = "";
        public string? FinishedAt { get; set; }
        public string Kind { get; set; } = "";
        public string? PortId { get; set; }
        public string StateBefore { get; set; } = "";
        public string? StateAfter { get; set; }
        public string? Outcome { get; set; }
        public string? Details { get; set; }
        public string CorrelationId { get; set; } = "";
    }
}
