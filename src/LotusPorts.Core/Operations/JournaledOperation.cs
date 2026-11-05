using LotusPorts.Core.Privacy;

namespace LotusPorts.Core.Operations;

public sealed record OperationStepResult(OperationOutcome Outcome, string? StateAfterJson, string? Details = null);

/// <summary>Runs a disk/system change between a BEFORE and an AFTER journal entry, also on failure and cancellation.</summary>
public static class JournaledOperation
{
    public static async Task<OperationStepResult> RunAsync(
        IOperationJournal journal,
        OperationKind kind,
        string? portId,
        string stateBeforeJson,
        Func<CancellationToken, Task<OperationStepResult>> body,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(body);
        var id = await journal.BeginAsync(kind, portId, stateBeforeJson, correlationId ?? Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
        try
        {
            var result = await body(cancellationToken).ConfigureAwait(false);
            await journal.CompleteAsync(id, result.Outcome, result.StateAfterJson, result.Details, CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException)
        {
            // The AFTER state is unknown: recorded as not obtained, never guessed.
            await journal.CompleteAsync(id, OperationOutcome.Cancelled, null, "Cancelled.", CancellationToken.None).ConfigureAwait(false);
            throw;
        }
        catch (Exception ex)
        {
            await journal.CompleteAsync(id, OperationOutcome.Failure, null, PathAnonymizer.Anonymize($"{ex.GetType().Name}: {ex.Message}"), CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }
}
