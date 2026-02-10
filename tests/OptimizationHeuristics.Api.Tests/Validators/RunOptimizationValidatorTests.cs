using FluentValidation.TestHelper;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Validators;

namespace OptimizationHeuristics.Api.Tests.Validators;

public class RunOptimizationValidatorTests
{
    private readonly RunOptimizationValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new RunOptimizationRequest(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_ConfigurationId_Fails()
    {
        var request = new RunOptimizationRequest(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AlgorithmConfigurationId);
    }

    [Fact]
    public void Empty_ProblemId_Fails()
    {
        var request = new RunOptimizationRequest(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ProblemDefinitionId);
    }
}
