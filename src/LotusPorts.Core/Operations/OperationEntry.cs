namespace LotusPorts.Core.Operations;

/// <summary>One row of the operation journal: state BEFORE, state AFTER and the outcome.</summary>
public sealed record OperationEntry(
    long Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    OperationKind Kind,
    string? PortId,
    string StateBefore,
    string? StateAfter,
    OperationOutcome? Outcome,
    string? Details,
    Guid CorrelationId)
{
    /// <summary>Started but never finished (app closed or crashed in the middle).</summary>
    public bool IsOpen => Outcome is null;
}

public interface IOperationJournal
{
    /// <summary>Writes the BEFORE state. <paramref name="stateBeforeJson"/> must be valid JSON.</summary>
    Task<long> BeginAsync(OperationKind kind, string? portId, string stateBeforeJson, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Writes the AFTER state and the outcome. <paramref name="stateAfterJson"/> is null when it could not be obtained.</summary>
    Task CompleteAsync(long id, OperationOutcome outcome, string? stateAfterJson, string? details, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OperationEntry>> ListRecentAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>Operations without an outcome. Called at startup, when nothing is running, they are the interrupted ones.</summary>
    Task<IReadOnlyList<OperationEntry>> ListOpenAsync(CancellationToken cancellationToken = default);
}
