using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class AntColonyOptimization : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        List<IterationResult> history)
    {
        var antCount = GetIntParam(parameters, "antCount", 20);
        var alpha = GetParam(parameters, "alpha", 1.0);
        var beta = GetParam(parameters, "beta", 5.0);
        var evaporationRate = GetParam(parameters, "evaporationRate", 0.5);
        var pheromoneDeposit = GetParam(parameters, "pheromoneDeposit", 100.0);

        var n = cities.Count;
        var distances = BuildDistanceMatrix(cities);
        var pheromones = new double[n, n];
        InitializePheromones(pheromones, n, 1.0);

        var bestRoute = GenerateRandomRoute(n, Rng);
        var bestDistance = Route.CalculateTotalDistance(bestRoute, cities);

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var iterationBestRoute = bestRoute;
            var iterationBestDistance = bestDistance;

            for (var ant = 0; ant < antCount; ant++)
            {
                var tour = ConstructTour(n, pheromones, distances, alpha, beta);
                var distance = Route.CalculateTotalDistance(tour, cities);

                if (distance < iterationBestDistance)
                {
                    iterationBestRoute = tour;
                    iterationBestDistance = distance;
                }
            }

            EvaporatePheromones(pheromones, n, evaporationRate);
            DepositPheromones(pheromones, iterationBestRoute, iterationBestDistance, pheromoneDeposit);

            if (iterationBestDistance < bestDistance)
            {
                bestRoute = new List<int>(iterationBestRoute);
                bestDistance = iterationBestDistance;
            }

            // iterationBestDistance = best ant found this iteration (noisy; can be > bestDistance)
            history.Add(new IterationResult(iteration, bestDistance, new List<int>(bestRoute), iterationBestDistance));
        }

        return (bestRoute, bestDistance);
    }

    private List<int> ConstructTour(int n, double[,] pheromones, double[,] distances, double alpha, double beta)
    {
        var visited = new bool[n];
        var tour = new List<int>(n);
        var start = Rng.Next(n);
        tour.Add(start);
        visited[start] = true;

        for (var step = 1; step < n; step++)
        {
            var current = tour[^1];
            var next = SelectNextCity(current, visited, pheromones, distances, n, alpha, beta);
            tour.Add(next);
            visited[next] = true;
        }

        return tour;
    }

    private int SelectNextCity(int current, bool[] visited, double[,] pheromones, double[,] distances, int n, double alpha, double beta)
    {
        var probabilities = new double[n];
        var sum = 0.0;

        for (var j = 0; j < n; j++)
        {
            if (visited[j]) continue;
            var dist = distances[current, j];
            if (dist == 0) dist = 0.0001;
            probabilities[j] = Math.Pow(pheromones[current, j], alpha) * Math.Pow(1.0 / dist, beta);
            sum += probabilities[j];
        }

        if (sum == 0)
        {
            for (var j = 0; j < n; j++)
                if (!visited[j]) return j;
        }

        var rand = Rng.NextDouble() * sum;
        var cumulative = 0.0;
        for (var j = 0; j < n; j++)
        {
            if (visited[j]) continue;
            cumulative += probabilities[j];
            if (cumulative >= rand) return j;
        }

        for (var j = 0; j < n; j++)
            if (!visited[j]) return j;

        return 0;
    }

    private static double[,] BuildDistanceMatrix(IReadOnlyList<City> cities)
    {
        var n = cities.Count;
        var matrix = new double[n, n];
        for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
            {
                var d = cities[i].DistanceTo(cities[j]);
                matrix[i, j] = d;
                matrix[j, i] = d;
            }
        return matrix;
    }

    private static void InitializePheromones(double[,] pheromones, int n, double value)
    {
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                pheromones[i, j] = value;
    }

    private static void EvaporatePheromones(double[,] pheromones, int n, double rate)
    {
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                pheromones[i, j] *= (1.0 - rate);
    }

    private static void DepositPheromones(double[,] pheromones, List<int> tour, double distance, double deposit)
    {
        var amount = deposit / distance;
        for (var i = 0; i < tour.Count - 1; i++)
        {
            pheromones[tour[i], tour[i + 1]] += amount;
            pheromones[tour[i + 1], tour[i]] += amount;
        }
        pheromones[tour[^1], tour[0]] += amount;
        pheromones[tour[0], tour[^1]] += amount;
    }
}
