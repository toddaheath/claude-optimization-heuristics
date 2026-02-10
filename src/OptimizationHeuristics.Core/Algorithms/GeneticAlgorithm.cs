using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class GeneticAlgorithm : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        List<IterationResult> history)
    {
        var populationSize = GetIntParam(parameters, "populationSize", 50);
        var mutationRate = GetParam(parameters, "mutationRate", 0.02);
        var tournamentSize = GetIntParam(parameters, "tournamentSize", 5);
        var eliteCount = GetIntParam(parameters, "eliteCount", 2);

        var n = cities.Count;
        var population = InitializePopulation(populationSize, n);
        var fitness = EvaluatePopulation(population, cities);

        var bestIdx = Array.IndexOf(fitness, fitness.Min());
        var bestRoute = new List<int>(population[bestIdx]);
        var bestDistance = fitness[bestIdx];

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var newPopulation = new List<List<int>>(populationSize);

            // Elitism
            var eliteIndices = fitness
                .Select((f, i) => (f, i))
                .OrderBy(x => x.f)
                .Take(eliteCount)
                .Select(x => x.i)
                .ToList();

            foreach (var idx in eliteIndices)
                newPopulation.Add(new List<int>(population[idx]));

            while (newPopulation.Count < populationSize)
            {
                var parent1 = TournamentSelect(population, fitness, tournamentSize);
                var parent2 = TournamentSelect(population, fitness, tournamentSize);
                var child = OrderCrossover(parent1, parent2, n);
                if (Rng.NextDouble() < mutationRate)
                    SwapMutation(child);
                newPopulation.Add(child);
            }

            population = newPopulation;
            fitness = EvaluatePopulation(population, cities);

            var iterBestIdx = Array.IndexOf(fitness, fitness.Min());
            if (fitness[iterBestIdx] < bestDistance)
            {
                bestRoute = new List<int>(population[iterBestIdx]);
                bestDistance = fitness[iterBestIdx];
            }

            history.Add(new IterationResult(iteration, bestDistance, new List<int>(bestRoute)));
        }

        return (bestRoute, bestDistance);
    }

    private List<List<int>> InitializePopulation(int size, int cityCount)
    {
        var population = new List<List<int>>(size);
        for (var i = 0; i < size; i++)
            population.Add(GenerateRandomRoute(cityCount, Rng));
        return population;
    }

    private static double[] EvaluatePopulation(List<List<int>> population, IReadOnlyList<City> cities)
    {
        return population.Select(route => Route.CalculateTotalDistance(route, cities)).ToArray();
    }

    private List<int> TournamentSelect(List<List<int>> population, double[] fitness, int tournamentSize)
    {
        var bestIdx = Rng.Next(population.Count);
        for (var i = 1; i < tournamentSize; i++)
        {
            var candidate = Rng.Next(population.Count);
            if (fitness[candidate] < fitness[bestIdx])
                bestIdx = candidate;
        }
        return population[bestIdx];
    }

    private List<int> OrderCrossover(List<int> parent1, List<int> parent2, int n)
    {
        var start = Rng.Next(n);
        var end = Rng.Next(start, n);

        var child = new int[n];
        Array.Fill(child, -1);
        var inChild = new bool[n];

        for (var i = start; i <= end; i++)
        {
            child[i] = parent1[i];
            inChild[parent1[i]] = true;
        }

        var pos = (end + 1) % n;
        for (var i = 0; i < n; i++)
        {
            var gene = parent2[(end + 1 + i) % n];
            if (!inChild[gene])
            {
                child[pos] = gene;
                pos = (pos + 1) % n;
            }
        }

        return child.ToList();
    }

    private void SwapMutation(List<int> route)
    {
        var i = Rng.Next(route.Count);
        var j = Rng.Next(route.Count);
        (route[i], route[j]) = (route[j], route[i]);
    }
}
