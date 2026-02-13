using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class TabuSearchTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new TabuSearch();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "tabuTenure", 5 },
        { "neighborhoodSize", 10 }
    };
}
