using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class SlimeMoldOptimizationTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new SlimeMoldOptimization();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "populationSize", 8 },
        { "z", 0.03 }
    };
}
