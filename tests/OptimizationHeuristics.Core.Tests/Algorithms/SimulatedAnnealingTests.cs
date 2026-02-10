using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class SimulatedAnnealingTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new SimulatedAnnealing();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "initialTemperature", 10000.0 },
        { "coolingRate", 0.99 },
        { "minTemperature", 0.01 }
    };
}
