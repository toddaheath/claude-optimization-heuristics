using System.Diagnostics;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public abstract class AlgorithmBase : IOptimizationAlgorithm
{
    protected Random Rng = new();

    public OptimizationResult Solve(IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        Action<IterationResult>? onIteration = null, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        IList<IterationResult> history = onIteration != null
            ? new ObservableList(onIteration)
            : new List<IterationResult>();

        var (bestRoute, bestDistance) = RunAlgorithm(cities, maxIterations, parameters, history, cancellationToken);

        sw.Stop();
        return new OptimizationResult
        {
            BestDistance = bestDistance,
            BestRoute = bestRoute,
            IterationHistory = history.ToList(),
            TotalIterations = history.Count,
            ExecutionTimeMs = sw.ElapsedMilliseconds
        };
    }

    // Wraps a List<IterationResult> so the callback fires on every Add via interface dispatch.
    private sealed class ObservableList : List<IterationResult>, IList<IterationResult>
    {
        private readonly Action<IterationResult> _callback;
        public ObservableList(Action<IterationResult> callback) => _callback = callback;
        void ICollection<IterationResult>.Add(IterationResult item) { base.Add(item); _callback(item); }
    }

    protected abstract (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities,
        int maxIterations,
        Dictionary<string, object> parameters,
        IList<IterationResult> history,
        CancellationToken cancellationToken = default);

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
            if (value is System.Text.Json.JsonElement je) return je.GetDouble();
            return Convert.ToDouble(value);
        }
        return defaultValue;
    }

    protected static int GetIntParam(Dictionary<string, object> parameters, string key, int defaultValue)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is System.Text.Json.JsonElement je) return je.GetInt32();
            return Convert.ToInt32(value);
        }
        return defaultValue;
    }
}
