using FluentAssertions;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Core.Tests.Services;

public class RunProgressStoreTests
{
    private readonly RunProgressStore _store = new();

    [Fact]
    public void InitRun_ThenGetSnapshot_ReturnsRunningWithEmptyHistory()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);

        var snapshot = _store.GetSnapshot(runId);

        snapshot.Should().NotBeNull();
        snapshot!.Status.Should().Be(RunStatus.Running);
        snapshot.IterationHistory.Should().BeEmpty();
        snapshot.BestDistance.Should().BeNull();
        snapshot.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void AddIteration_AccumulatesEntries()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);

        _store.AddIteration(runId, new IterationResult(0, 100.0, new List<int> { 0, 1, 2 }, 100.0));
        _store.AddIteration(runId, new IterationResult(1, 90.0, new List<int> { 0, 2, 1 }, 90.0));

        var snapshot = _store.GetSnapshot(runId);
        snapshot!.IterationHistory.Should().HaveCount(2);
        snapshot.IterationHistory[1].BestDistance.Should().Be(90.0);
    }

    [Fact]
    public void CompleteRun_TransitionsStatusAndRecordsDistanceAndTime()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);
        _store.AddIteration(runId, new IterationResult(0, 50.0, new List<int> { 0, 1, 2 }, 50.0));

        _store.CompleteRun(runId, 50.0, 1234);

        var snapshot = _store.GetSnapshot(runId);
        snapshot!.Status.Should().Be(RunStatus.Completed);
        snapshot.BestDistance.Should().Be(50.0);
        snapshot.ExecutionTimeMs.Should().Be(1234);
    }

    [Fact]
    public void FailRun_TransitionsStatusAndRecordsError()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);

        _store.FailRun(runId, "something broke");

        var snapshot = _store.GetSnapshot(runId);
        snapshot!.Status.Should().Be(RunStatus.Failed);
        snapshot.ErrorMessage.Should().Be("something broke");
    }

    [Fact]
    public void GetSnapshot_UnknownRunId_ReturnsNull()
    {
        _store.GetSnapshot(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public void CleanUp_RemovesEntry()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);
        _store.GetSnapshot(runId).Should().NotBeNull();

        _store.CleanUp(runId);

        _store.GetSnapshot(runId).Should().BeNull();
    }

    [Fact]
    public void CancelRun_CancelsToken()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);
        var token = _store.GetCancellationToken(runId);
        token.IsCancellationRequested.Should().BeFalse();

        _store.CancelRun(runId);

        token.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void CancelAll_CancelsAllActiveRuns()
    {
        var run1 = Guid.NewGuid();
        var run2 = Guid.NewGuid();
        _store.InitRun(run1);
        _store.InitRun(run2);
        var token1 = _store.GetCancellationToken(run1);
        var token2 = _store.GetCancellationToken(run2);

        _store.CancelAll();

        token1.IsCancellationRequested.Should().BeTrue();
        token2.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void ConcurrentAddIteration_FromMultipleThreads_DoesNotLoseEntries()
    {
        var runId = Guid.NewGuid();
        _store.InitRun(runId);

        const int threadCount = 10;
        const int iterationsPerThread = 100;

        var tasks = Enumerable.Range(0, threadCount).Select(t =>
            Task.Run(() =>
            {
                for (var i = 0; i < iterationsPerThread; i++)
                {
                    _store.AddIteration(runId, new IterationResult(
                        t * iterationsPerThread + i, 100.0 - i, new List<int> { 0, 1 }, 100.0 - i));
                }
            })).ToArray();

        Task.WaitAll(tasks);

        var snapshot = _store.GetSnapshot(runId);
        snapshot!.IterationHistory.Should().HaveCount(threadCount * iterationsPerThread);
    }
}
