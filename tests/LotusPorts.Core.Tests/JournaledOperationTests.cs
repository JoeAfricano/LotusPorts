using LotusPorts.Core.Operations;
using NSubstitute;

namespace LotusPorts.Core.Tests;

public class JournaledOperationTests
{
    private readonly IOperationJournal journal = Substitute.For<IOperationJournal>();

    public JournaledOperationTests() =>
        journal.BeginAsync(default, default, default!, default, default).ReturnsForAnyArgs(42L);

    [Fact]
    public async Task Success_writes_before_then_after()
    {
        var result = await JournaledOperation.RunAsync(journal, OperationKind.Install, "port", """{"v":null}""",
            _ => Task.FromResult(new OperationStepResult(OperationOutcome.Success, """{"v":"1"}""")));

        result.Outcome.Should().Be(OperationOutcome.Success);
        Received.InOrder(() =>
        {
            journal.BeginAsync(OperationKind.Install, "port", """{"v":null}""", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            journal.CompleteAsync(42, OperationOutcome.Success, """{"v":"1"}""", null, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Failure_is_recorded_without_guessing_the_after_state()
    {
        var act = () => JournaledOperation.RunAsync(journal, OperationKind.Update, "port", "{}",
            _ => throw new IOException("disk full"));

        await act.Should().ThrowAsync<IOException>();
        await journal.Received(1).CompleteAsync(42, OperationOutcome.Failure, Arg.Is<string?>(s => s == null), Arg.Is<string?>(d => d!.Contains("disk full")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancellation_is_recorded_as_cancelled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var act = () => JournaledOperation.RunAsync(journal, OperationKind.Install, null, "{}",
            ct => { ct.ThrowIfCancellationRequested(); return Task.FromResult(new OperationStepResult(OperationOutcome.Success, "{}")); },
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        await journal.Received(1).CompleteAsync(42, OperationOutcome.Cancelled, Arg.Is<string?>(s => s == null), Arg.Any<string?>(), CancellationToken.None);
    }

    [Fact]
    public async Task Correlation_id_is_passed_through()
    {
        var id = Guid.NewGuid();
        await JournaledOperation.RunAsync(journal, OperationKind.Remove, "p", "{}",
            _ => Task.FromResult(new OperationStepResult(OperationOutcome.Success, "{}")), id);

        await journal.Received(1).BeginAsync(OperationKind.Remove, "p", "{}", id, Arg.Any<CancellationToken>());
    }
}
