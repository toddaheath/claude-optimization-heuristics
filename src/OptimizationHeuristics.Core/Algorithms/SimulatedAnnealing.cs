using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class SimulatedAnnealing : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        IList<IterationResult> history, CancellationToken cancellationToken = default)
    {
        var initialTemp = GetParam(parameters, "initialTemperature", 10000.0);
        var coolingRate = GetParam(parameters, "coolingRate", 0.995);
        var minTemp = GetParam(parameters, "minTemperature", 0.01);

        var currentRoute = GenerateRandomRoute(cities.Count, Rng);
        var currentDistance = Route.CalculateTotalDistance(currentRoute, cities);

        var bestRoute = new List<int>(currentRoute);
        var bestDistance = currentDistance;
        var temperature = initialTemp;

        for (var iteration = 0; iteration < maxIterations && temperature > minTemp && !cancellationToken.IsCancellationRequested; iteration++)
        {
            // Pick two random indices for the 2-opt segment reversal
            var i = Rng.Next(currentRoute.Count);
            var j = Rng.Next(currentRoute.Count);
            if (i > j) (i, j) = (j, i);

            // Reverse segment in-place (no allocation)
            ReverseSegment(currentRoute, i, j);
            var newDistance = Route.CalculateTotalDistance(currentRoute, cities);
            var delta = newDistance - currentDistance;

            if (delta < 0 || Rng.NextDouble() < Math.Exp(-delta / temperature))
            {
                // Accept the move — keep the reversed segment
                currentDistance = newDistance;
            }
            else
            {
                // Reject the move — reverse back to restore original route
                ReverseSegment(currentRoute, i, j);
            }

            if (currentDistance < bestDistance)
            {
                bestRoute = new List<int>(currentRoute);
                bestDistance = currentDistance;
            }

            temperature *= coolingRate;

            history.Add(new IterationResult(iteration, bestDistance, new List<int>(bestRoute), currentDistance));
        }

        return (bestRoute, bestDistance);
    }

    /// <summary>
    /// Reverses the segment of the route between indices i and j (inclusive) in-place.
    /// Calling this twice with the same indices restores the original order.
    /// </summary>
    private static void ReverseSegment(List<int> route, int i, int j)
    {
        while (i < j)
        {
            (route[i], route[j]) = (route[j], route[i]);
            i++;
            j--;
        }
    }
}
