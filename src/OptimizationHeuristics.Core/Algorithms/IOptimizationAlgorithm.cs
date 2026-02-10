using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public interface IOptimizationAlgorithm
{
    OptimizationResult Solve(IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters);
}
