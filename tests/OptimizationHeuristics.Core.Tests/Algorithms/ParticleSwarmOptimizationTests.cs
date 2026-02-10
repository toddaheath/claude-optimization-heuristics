using OptimizationHeuristics.Core.Algorithms;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class ParticleSwarmOptimizationTests : AlgorithmTestBase
{
    protected override IOptimizationAlgorithm CreateAlgorithm() => new ParticleSwarmOptimization();

    protected override Dictionary<string, object> GetParameters() => new()
    {
        { "swarmSize", 8 },
        { "cognitiveWeight", 2.0 },
        { "socialWeight", 2.0 }
    };
}
