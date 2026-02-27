using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Services;

public record RunProgressSnapshot(
    Guid RunId,
    RunStatus Status,
    List<IterationResult> IterationHistory,
    double? BestDistance,
    long ExecutionTimeMs,
    string? ErrorMessage
);

public interface IRunProgressStore
{
    void InitRun(Guid runId);
    void AddIteration(Guid runId, IterationResult result);
    void CompleteRun(Guid runId, double bestDistance, long executionTimeMs);
    void FailRun(Guid runId, string errorMessage);
    RunProgressSnapshot? GetSnapshot(Guid runId);
    void CleanUp(Guid runId);
    CancellationToken GetCancellationToken(Guid runId);
    void CancelRun(Guid runId);
    void CancelAll();
}
