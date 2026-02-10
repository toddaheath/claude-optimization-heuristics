namespace OptimizationHeuristics.Core.Models;

public class OptimizationResult
{
    public double BestDistance { get; init; }
    public List<int> BestRoute { get; init; } = new();
    public List<IterationResult> IterationHistory { get; init; } = new();
    public int TotalIterations { get; init; }
    public long ExecutionTimeMs { get; init; }
}
