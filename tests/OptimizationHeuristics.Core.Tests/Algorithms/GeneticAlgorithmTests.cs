using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class GeneticAlgorithmTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new GeneticAlgorithm();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "populationSize", 10 },
        { "mutationRate", 0.05 },
        { "tournamentSize", 3 },
        { "eliteCount", 2 }
    };
}
