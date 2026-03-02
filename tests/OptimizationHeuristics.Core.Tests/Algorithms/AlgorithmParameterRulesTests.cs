using System.Text.Json;
using FluentAssertions;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Tests.Algorithms;

public class AlgorithmParameterRulesTests
{
    [Fact]
    public void Validate_EmptyParameters_ReturnsNoErrors()
    {
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, new Dictionary<string, object>());
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullParameters_ReturnsNoErrors()
    {
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, null);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidSAParameters_ReturnsNoErrors()
    {
        var parameters = new Dictionary<string, object>
        {
            { "initialTemperature", 1000.0 },
            { "coolingRate", 0.95 },
            { "minTemperature", 0.1 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_SA_CoolingRateOutOfRange_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "coolingRate", 1.5 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().ContainSingle().Which.Should().Contain("coolingRate");
    }

    [Fact]
    public void Validate_SA_NegativeTemperature_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "initialTemperature", -100.0 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().ContainSingle().Which.Should().Contain("initialTemperature");
    }

    [Fact]
    public void Validate_UnknownParameter_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "unknownParam", 42.0 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().ContainSingle().Which.Should().Contain("Unknown parameter 'unknownParam'");
    }

    [Fact]
    public void Validate_GA_TournamentSizeExceedsPopulation_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "populationSize", 10 },
            { "tournamentSize", 20 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.GeneticAlgorithm, parameters);
        errors.Should().Contain(e => e.Contains("tournamentSize must be less than or equal to populationSize"));
    }

    [Fact]
    public void Validate_GA_EliteCountEqualToPopulation_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "populationSize", 10 },
            { "eliteCount", 10 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.GeneticAlgorithm, parameters);
        errors.Should().Contain(e => e.Contains("eliteCount must be less than populationSize"));
    }

    [Fact]
    public void Validate_PSO_InertiaMinGreaterThanMax_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "inertiaMin", 0.9 },
            { "inertiaMax", 0.4 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.ParticleSwarmOptimization, parameters);
        errors.Should().Contain(e => e.Contains("inertiaMin must be less than inertiaMax"));
    }

    [Fact]
    public void Validate_ACO_ValidParameters_ReturnsNoErrors()
    {
        var parameters = new Dictionary<string, object>
        {
            { "antCount", 20 },
            { "alpha", 1.0 },
            { "beta", 5.0 },
            { "evaporationRate", 0.5 },
            { "pheromoneDeposit", 100.0 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.AntColonyOptimization, parameters);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_TabuSearch_NegativeTenure_ReturnsError()
    {
        var parameters = new Dictionary<string, object>
        {
            { "tabuTenure", -5 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.TabuSearch, parameters);
        errors.Should().ContainSingle().Which.Should().Contain("tabuTenure");
    }

    [Fact]
    public void Validate_SlimeMold_ValidParameters_ReturnsNoErrors()
    {
        var parameters = new Dictionary<string, object>
        {
            { "populationSize", 30 },
            { "z", 0.03 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SlimeMoldOptimization, parameters);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_JsonElement_Values_WorkCorrectly()
    {
        var json = JsonSerializer.Serialize(new { coolingRate = 0.95, initialTemperature = 1000.0 });
        var doc = JsonDocument.Parse(json);
        var parameters = new Dictionary<string, object>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            parameters[prop.Name] = prop.Value;

        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void GetRules_AllAlgorithmTypes_HaveRules()
    {
        foreach (var type in Enum.GetValues<AlgorithmType>())
        {
            var rules = AlgorithmParameterRules.GetRules(type);
            rules.Should().NotBeEmpty($"algorithm {type} should have parameter rules defined");
        }
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAll()
    {
        var parameters = new Dictionary<string, object>
        {
            { "initialTemperature", -1.0 },
            { "coolingRate", 5.0 },
            { "bogusParam", 42.0 }
        };
        var errors = AlgorithmParameterRules.Validate(AlgorithmType.SimulatedAnnealing, parameters);
        errors.Should().HaveCount(3);
    }
}
