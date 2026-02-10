using FluentAssertions;
using FluentValidation.TestHelper;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Validators;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Api.Tests.Validators;

public class AlgorithmConfigurationValidatorTests
{
    private readonly CreateAlgorithmConfigurationValidator _createValidator = new();
    private readonly UpdateAlgorithmConfigurationValidator _updateValidator = new();

    [Fact]
    public void Create_Valid_Request_Passes()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "SA Config", "Test", AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 100);

        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_Empty_Name_Fails()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 100);

        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Create_Zero_MaxIterations_Fails()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "Test", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 0);

        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MaxIterations);
    }

    [Fact]
    public void Update_Valid_Request_Passes()
    {
        var request = new UpdateAlgorithmConfigurationRequest(
            "Updated Config", "Test", AlgorithmType.GeneticAlgorithm,
            new Dictionary<string, object>(), 500);

        var result = _updateValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_ExcessiveIterations_Fails()
    {
        var request = new UpdateAlgorithmConfigurationRequest(
            "Test", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 200000);

        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MaxIterations);
    }
}
