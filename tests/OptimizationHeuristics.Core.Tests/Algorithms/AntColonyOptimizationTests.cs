using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class AntColonyOptimizationTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new AntColonyOptimization();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "antCount", 5 },
        { "alpha", 1.0 },
        { "beta", 5.0 },
        { "evaporationRate", 0.5 },
        { "pheromoneDeposit", 100.0 }
    };
}
