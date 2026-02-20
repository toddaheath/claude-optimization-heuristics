namespace OptimizationHeuristics.Core.Models;

public record IterationResult(
    int Iteration,
    double BestDistance,
    List<int> BestRoute,
    double CurrentDistance  // candidate distance at this iteration (noisy; can exceed BestDistance)
);
