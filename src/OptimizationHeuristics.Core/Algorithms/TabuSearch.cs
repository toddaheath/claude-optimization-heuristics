using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class TabuSearch : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        IList<IterationResult> history, CancellationToken cancellationToken = default)
    {
        var tabuTenure = GetIntParam(parameters, "tabuTenure", 10);
        var neighborhoodSize = GetIntParam(parameters, "neighborhoodSize", 50);

        var currentRoute = GenerateRandomRoute(cities.Count, Rng);
        var currentDistance = Route.CalculateTotalDistance(currentRoute, cities);

        var bestRoute = new List<int>(currentRoute);
        var bestDistance = currentDistance;

        var tabuList = new Queue<(int, int)>();
        var tabuSet = new HashSet<(int, int)>();

        for (var iteration = 0; iteration < maxIterations && !cancellationToken.IsCancellationRequested; iteration++)
        {
            List<int>? bestNeighbor = null;
            var bestNeighborDistance = double.MaxValue;
            (int, int) bestMove = (0, 0);

            for (var n = 0; n < neighborhoodSize; n++)
            {
                var i = Rng.Next(cities.Count);
                var j = Rng.Next(cities.Count);
                if (i == j) continue;
                if (i > j) (i, j) = (j, i);

                var neighbor = TwoOptSwap(currentRoute, i, j);
                var neighborDistance = Route.CalculateTotalDistance(neighbor, cities);

                var isTabu = tabuSet.Contains((i, j));
                var aspirationMet = neighborDistance < bestDistance;

                if (neighborDistance < bestNeighborDistance && (!isTabu || aspirationMet))
                {
                    bestNeighbor = neighbor;
                    bestNeighborDistance = neighborDistance;
                    bestMove = (i, j);
                }
            }

            if (bestNeighbor != null)
            {
                currentRoute = bestNeighbor;
                currentDistance = bestNeighborDistance;

                tabuList.Enqueue(bestMove);
                tabuSet.Add(bestMove);
                if (tabuList.Count > tabuTenure)
                {
                    var removed = tabuList.Dequeue();
                    tabuSet.Remove(removed);
                }

                if (currentDistance < bestDistance)
                {
                    bestRoute = new List<int>(currentRoute);
                    bestDistance = currentDistance;
                }
            }

            // currentDistance = the accepted neighbour's distance (may exceed bestDistance)
            history.Add(new IterationResult(iteration, bestDistance, new List<int>(bestRoute), currentDistance));
        }

        return (bestRoute, bestDistance);
    }

    private static List<int> TwoOptSwap(List<int> route, int i, int j)
    {
        var newRoute = new List<int>(route);
        while (i < j)
        {
            (newRoute[i], newRoute[j]) = (newRoute[j], newRoute[i]);
            i++;
            j--;
        }
        return newRoute;
    }
}
