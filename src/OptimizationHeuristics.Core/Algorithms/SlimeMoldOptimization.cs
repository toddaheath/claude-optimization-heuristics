using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class SlimeMoldOptimization : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        IList<IterationResult> history, CancellationToken cancellationToken = default)
    {
        var populationSize = GetIntParam(parameters, "populationSize", 30);
        var z = GetParam(parameters, "z", 0.03);

        var n = cities.Count;
        var population = new List<List<int>>(populationSize);
        var fitness = new double[populationSize];

        for (var i = 0; i < populationSize; i++)
        {
            population.Add(GenerateRandomRoute(n, Rng));
            fitness[i] = Route.CalculateTotalDistance(population[i], cities);
        }

        var bestIdx = Array.IndexOf(fitness, fitness.Min());
        var bestRoute = new List<int>(population[bestIdx]);
        var bestDistance = fitness[bestIdx];

        for (var iteration = 0; iteration < maxIterations && !cancellationToken.IsCancellationRequested; iteration++)
        {
            var sortedIndices = Enumerable.Range(0, populationSize)
                .OrderBy(i => fitness[i]).ToArray();
            var bestFitness = fitness[sortedIndices[0]];
            var worstFitness = fitness[sortedIndices[^1]];

            // Precompute rank array: ranks[i] = position of individual i in sorted order
            // This replaces O(n) Array.IndexOf lookups with O(1) array access
            var ranks = new int[populationSize];
            for (var r = 0; r < sortedIndices.Length; r++)
                ranks[sortedIndices[r]] = r;

            var t = (double)(iteration + 1) / maxIterations;
            var a = Math.Atanh(Math.Min(1.0 - t, 0.999));

            for (var i = 0; i < populationSize; i++)
            {
                var weight = ComputeWeight(fitness[i], bestFitness, worstFitness, ranks[i], populationSize);

                var newRoute = new List<int>(population[i]);

                if (Rng.NextDouble() < z)
                {
                    // Random exploration
                    newRoute = GenerateRandomRoute(n, Rng);
                }
                else
                {
                    var p = Math.Tanh(Math.Abs(fitness[i] - bestFitness));
                    var vb = 2 * a * (Rng.NextDouble() - 0.5);
                    var vc = 2 * a * (Rng.NextDouble() - 0.5);

                    if (Rng.NextDouble() < p)
                    {
                        // Move toward best with weight influence
                        var swapCount = Math.Clamp((int)(Math.Abs(weight * vb) * n / 4), 1, n);
                        for (var s = 0; s < swapCount; s++)
                        {
                            var idx1 = Rng.Next(n);
                            var idx2 = Rng.Next(n);
                            (newRoute[idx1], newRoute[idx2]) = (newRoute[idx2], newRoute[idx1]);
                        }

                        // Incorporate best route elements
                        ApplyPartialRoute(newRoute, bestRoute, n, Rng);
                    }
                    else
                    {
                        // Explore around current position
                        var swapCount = Math.Clamp((int)(Math.Abs(vc) * n / 4), 1, n);
                        for (var s = 0; s < swapCount; s++)
                        {
                            var idx1 = Rng.Next(n);
                            var idx2 = Rng.Next(n);
                            (newRoute[idx1], newRoute[idx2]) = (newRoute[idx2], newRoute[idx1]);
                        }
                    }
                }

                var newDistance = Route.CalculateTotalDistance(newRoute, cities);
                if (newDistance < fitness[i])
                {
                    population[i] = newRoute;
                    fitness[i] = newDistance;
                }
            }

            var iterBestIdx = Array.IndexOf(fitness, fitness.Min());
            if (fitness[iterBestIdx] < bestDistance)
            {
                bestRoute = new List<int>(population[iterBestIdx]);
                bestDistance = fitness[iterBestIdx];
            }

            // fitness[iterBestIdx] = best individual in the population this iteration
            history.Add(new IterationResult(iteration, bestDistance, new List<int>(bestRoute), fitness[iterBestIdx]));
        }

        return (bestRoute, bestDistance);
    }

    private double ComputeWeight(double currentFitness, double bestFitness, double worstFitness,
        int rank, int populationSize)
    {
        var range = worstFitness - bestFitness;
        if (range == 0) return 1.0;

        if (rank < populationSize / 2)
        {
            return 1.0 + Rng.NextDouble() * Math.Log10((currentFitness - bestFitness) / range + 1);
        }
        else
        {
            return 1.0 - Rng.NextDouble() * Math.Log10((currentFitness - bestFitness) / range + 1);
        }
    }

    private static void ApplyPartialRoute(List<int> route, List<int> bestRoute, int n, Random rng)
    {
        var segmentLength = Math.Max(2, n / 5);
        var start = rng.Next(n - segmentLength);

        var inSegment = new HashSet<int>(segmentLength);
        for (var i = start; i < start + segmentLength; i++)
            inSegment.Add(bestRoute[i]);

        var remaining = new List<int>(n - segmentLength);
        for (var i = 0; i < n; i++)
            if (!inSegment.Contains(route[i]))
                remaining.Add(route[i]);

        var insertPos = Math.Min(start, remaining.Count);
        var idx = 0;
        for (var i = 0; i < insertPos; i++)
            route[i] = remaining[idx++];
        for (var i = start; i < start + segmentLength; i++)
            route[insertPos + (i - start)] = bestRoute[i];
        for (var i = insertPos + segmentLength; i < n; i++)
            route[i] = remaining[idx++];
    }
}
