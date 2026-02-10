using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Algorithms;

public static class AlgorithmFactory
{
    public static IOptimizationAlgorithm Create(AlgorithmType type) => type switch
    {
        AlgorithmType.SimulatedAnnealing => new SimulatedAnnealing(),
        AlgorithmType.AntColonyOptimization => new AntColonyOptimization(),
        AlgorithmType.GeneticAlgorithm => new GeneticAlgorithm(),
        AlgorithmType.ParticleSwarmOptimization => new ParticleSwarmOptimization(),
        AlgorithmType.SlimeMoldOptimization => new SlimeMoldOptimization(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown algorithm type: {type}")
    };
}
