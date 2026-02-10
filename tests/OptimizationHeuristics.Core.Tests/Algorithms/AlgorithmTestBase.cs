using FluentAssertions;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public abstract class AlgorithmTestBase
{
    protected abstract IOptimizationAlgorithm CreateAlgorithm();
    protected virtual Dictionary<string, object> GetParameters() => new();

    protected static List<City> CreateSquareCities() => new()
    {
        new City(0, 0.0, 0.0),
        new City(1, 1.0, 0.0),
        new City(2, 1.0, 1.0),
        new City(3, 0.0, 1.0)
    };

    protected static List<City> CreateRandomCities(int count, int seed = 42)
    {
        var rng = new Random(seed);
        return Enumerable.Range(0, count)
            .Select(i => new City(i, rng.NextDouble() * 100, rng.NextDouble() * 100))
            .ToList();
    }

    [Fact]
    public void Solve_ReturnsValidRoute()
    {
        var cities = CreateSquareCities();
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 20, GetParameters());

        result.BestRoute.Should().HaveCount(cities.Count);
        result.BestRoute.Distinct().Should().HaveCount(cities.Count);
        result.BestRoute.All(i => i >= 0 && i < cities.Count).Should().BeTrue();
    }

    [Fact]
    public void Solve_ReturnsPositiveDistance()
    {
        var cities = CreateSquareCities();
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 20, GetParameters());

        result.BestDistance.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Solve_RecordsIterationHistory()
    {
        var cities = CreateSquareCities();
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 20, GetParameters());

        result.IterationHistory.Should().NotBeEmpty();
        result.TotalIterations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Solve_ConvergesOnLargerProblem()
    {
        var cities = CreateRandomCities(8);
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 30, GetParameters());

        var firstDistance = result.IterationHistory.First().BestDistance;
        var lastDistance = result.IterationHistory.Last().BestDistance;
        lastDistance.Should().BeLessThanOrEqualTo(firstDistance);
    }

    [Fact]
    public void Solve_BestDistanceMatchesRouteCalculation()
    {
        var cities = CreateRandomCities(6);
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 20, GetParameters());

        var calculatedDistance = Route.CalculateTotalDistance(result.BestRoute, cities);
        result.BestDistance.Should().BeApproximately(calculatedDistance, 0.0001);
    }

    [Fact]
    public void Solve_RecordsExecutionTime()
    {
        var cities = CreateSquareCities();
        var algo = CreateAlgorithm();

        var result = algo.Solve(cities, 10, GetParameters());

        result.ExecutionTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
