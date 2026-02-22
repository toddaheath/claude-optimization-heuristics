using System.Collections.Concurrent;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Services;

public class RunProgressStore : IRunProgressStore
{
    private sealed class RunState
    {
        public RunStatus Status = RunStatus.Running;
        public readonly List<IterationResult> History = [];
        public double? BestDistance;
        public long ExecutionTimeMs;
        public string? ErrorMessage;
        public readonly object Lock = new();
    }

    private readonly ConcurrentDictionary<Guid, RunState> _store = new();

    public void InitRun(Guid runId) =>
        _store[runId] = new RunState();

    public void AddIteration(Guid runId, IterationResult result)
    {
        if (!_store.TryGetValue(runId, out var state)) return;
        lock (state.Lock) state.History.Add(result);
    }

    public void CompleteRun(Guid runId, double bestDistance, long executionTimeMs)
    {
        if (!_store.TryGetValue(runId, out var state)) return;
        lock (state.Lock)
        {
            state.Status = RunStatus.Completed;
            state.BestDistance = bestDistance;
            state.ExecutionTimeMs = executionTimeMs;
        }
    }

    public void FailRun(Guid runId, string errorMessage)
    {
        if (!_store.TryGetValue(runId, out var state)) return;
        lock (state.Lock)
        {
            state.Status = RunStatus.Failed;
            state.ErrorMessage = errorMessage;
        }
    }

    public RunProgressSnapshot? GetSnapshot(Guid runId)
    {
        if (!_store.TryGetValue(runId, out var state)) return null;
        lock (state.Lock)
        {
            return new RunProgressSnapshot(
                runId,
                state.Status,
                [.. state.History],
                state.BestDistance,
                state.ExecutionTimeMs,
                state.ErrorMessage
            );
        }
    }

    public void CleanUp(Guid runId) => _store.TryRemove(runId, out _);
}
