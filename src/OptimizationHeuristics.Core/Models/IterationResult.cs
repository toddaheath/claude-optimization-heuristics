namespace OptimizationHeuristics.Core.Models;

public record IterationResult(
    int Iteration,
    double BestDistance,
    List<int> BestRoute
);
