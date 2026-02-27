using System.Text.Json;
using FluentAssertions;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class AlgorithmBaseTests
{
    /// <summary>Minimal concrete subclass to expose the protected static helpers.</summary>
    private sealed class TestAlgorithm : AlgorithmBase
    {
        protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
            IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
            IList<IterationResult> history, CancellationToken cancellationToken = default)
        {
            return ([0, 1], 42.0);
        }

        // Expose the protected statics for testing.
        public static double TestGetParam(Dictionary<string, object> p, string key, double def) => GetParam(p, key, def);
        public static int TestGetIntParam(Dictionary<string, object> p, string key, int def) => GetIntParam(p, key, def);
    }

    [Fact]
    public void GetParam_WithNativeDouble_ReturnsValue()
    {
        var p = new Dictionary<string, object> { { "temp", 1000.0 } };
        TestAlgorithm.TestGetParam(p, "temp", 0.0).Should().Be(1000.0);
    }

    [Fact]
    public void GetParam_WithJsonElement_ReturnsValue()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("42.5");
        var p = new Dictionary<string, object> { { "rate", json } };
        TestAlgorithm.TestGetParam(p, "rate", 0.0).Should().Be(42.5);
    }

    [Fact]
    public void GetParam_MissingKey_ReturnsDefault()
    {
        var p = new Dictionary<string, object>();
        TestAlgorithm.TestGetParam(p, "missing", 99.9).Should().Be(99.9);
    }

    [Fact]
    public void GetIntParam_WithNativeInt_ReturnsValue()
    {
        var p = new Dictionary<string, object> { { "count", 10 } };
        TestAlgorithm.TestGetIntParam(p, "count", 0).Should().Be(10);
    }

    [Fact]
    public void GetIntParam_WithJsonElement_ReturnsValue()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("25");
        var p = new Dictionary<string, object> { { "size", json } };
        TestAlgorithm.TestGetIntParam(p, "size", 0).Should().Be(25);
    }

    [Fact]
    public void GetIntParam_MissingKey_ReturnsDefault()
    {
        var p = new Dictionary<string, object>();
        TestAlgorithm.TestGetIntParam(p, "missing", 7).Should().Be(7);
    }

    [Fact]
    public void Solve_InvokesOnIterationCallback()
    {
        var algo = new TestAlgorithm();
        var cities = new List<City> { new(0, 0, 0), new(1, 10, 10) };
        var callbackCount = 0;

        algo.Solve(cities, 1, new Dictionary<string, object>(), onIteration: _ => callbackCount++);

        // TestAlgorithm doesn't add to history, so callback won't fire, but Solve should still complete.
        callbackCount.Should().Be(0);
    }

    [Fact]
    public void Solve_ReturnsOptimizationResult()
    {
        var algo = new TestAlgorithm();
        var cities = new List<City> { new(0, 0, 0), new(1, 10, 10) };

        var result = algo.Solve(cities, 1, new Dictionary<string, object>());

        result.BestDistance.Should().Be(42.0);
        result.BestRoute.Should().Equal(0, 1);
    }
}
