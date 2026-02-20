using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class SimulatedAnnealing : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        List<IterationResult> history)
    {
        var initialTemp = GetParam(parameters, "initialTemperature", 10000.0);
        var coolingRate = GetParam(parameters, "coolingRate", 0.995);
        var minTemp = GetParam(parameters, "minTemperature", 0.01);

        var currentRoute = GenerateRandomRoute(cities.Count, Rng);
        var currentDistance = Route.CalculateTotalDistance(currentRoute, cities);

        var bestRoute = new List<int>(currentRoute);
        var bestDistance = currentDistance;
        var temperature = initialTemp;

        for (var iteration = 0; iteration < maxIterations && temperature > minTemp; iteration++)
        {
            var newRoute = TwoOptSwap(currentRoute);
            var newDistance = Route.CalculateTotalDistance(newRoute, cities);
            var delta = newDistance - currentDistance;

            if (delta < 0 || Rng.NextDouble() < Math.Exp(-delta / temperature))
            {
                currentRoute = newRoute;
                currentDistance = newDistance;
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

    private List<int> TwoOptSwap(List<int> route)
    {
        var newRoute = new List<int>(route);
        var i = Rng.Next(newRoute.Count);
        var j = Rng.Next(newRoute.Count);
        if (i > j) (i, j) = (j, i);
        while (i < j)
        {
            (newRoute[i], newRoute[j]) = (newRoute[j], newRoute[i]);
            i++;
            j--;
        }
        return newRoute;
    }
}
