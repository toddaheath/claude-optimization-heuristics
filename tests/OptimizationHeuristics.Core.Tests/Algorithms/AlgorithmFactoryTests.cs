using FluentAssertions;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class AlgorithmFactoryTests
{
    [Theory]
    [InlineData(AlgorithmType.SimulatedAnnealing, typeof(SimulatedAnnealing))]
    [InlineData(AlgorithmType.AntColonyOptimization, typeof(AntColonyOptimization))]
    [InlineData(AlgorithmType.GeneticAlgorithm, typeof(GeneticAlgorithm))]
    [InlineData(AlgorithmType.ParticleSwarmOptimization, typeof(ParticleSwarmOptimization))]
    [InlineData(AlgorithmType.SlimeMoldOptimization, typeof(SlimeMoldOptimization))]
    [InlineData(AlgorithmType.TabuSearch, typeof(TabuSearch))]
    public void Create_ReturnsCorrectAlgorithmType(AlgorithmType type, Type expectedType)
    {
        var algorithm = AlgorithmFactory.Create(type);
        algorithm.Should().BeOfType(expectedType);
    }

    [Fact]
    public void Create_InvalidType_ThrowsArgumentOutOfRangeException()
    {
        var act = () => AlgorithmFactory.Create((AlgorithmType)999);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
