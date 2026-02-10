using System.Diagnostics;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public abstract class AlgorithmBase : IOptimizationAlgorithm
{
    protected Random Rng = new();

    public OptimizationResult Solve(IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters)
    {
        var sw = Stopwatch.StartNew();
        var history = new List<IterationResult>();

        var (bestRoute, bestDistance) = RunAlgorithm(cities, maxIterations, parameters, history);

        sw.Stop();
        return new OptimizationResult
        {
            BestDistance = bestDistance,
            BestRoute = bestRoute,
            IterationHistory = history,
            TotalIterations = history.Count,
            ExecutionTimeMs = sw.ElapsedMilliseconds
        };
    }

    protected abstract (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities,
        int maxIterations,
        Dictionary<string, object> parameters,
        List<IterationResult> history);

    protected static List<int> GenerateRandomRoute(int cityCount, Random rng)
    {
        var route = Enumerable.Range(0, cityCount).ToList();
        for (var i = route.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (route[i], route[j]) = (route[j], route[i]);
        }
        return route;
    }

    protected static double GetParam(Dictionary<string, object> parameters, string key, double defaultValue)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            return Convert.ToDouble(value);
        }
        return defaultValue;
    }

    protected static int GetIntParam(Dictionary<string, object> parameters, string key, int defaultValue)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            return Convert.ToInt32(value);
        }
        return defaultValue;
    }
}
