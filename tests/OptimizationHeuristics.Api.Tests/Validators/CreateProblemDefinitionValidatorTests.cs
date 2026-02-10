using FluentAssertions;
using FluentValidation.TestHelper;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Validators;

namespace OptimizationHeuristics.Api.Tests.Validators;

public class CreateProblemDefinitionValidatorTests
{
    private readonly CreateProblemDefinitionValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new CreateProblemDefinitionRequest(
            "Test Problem", "Description",
            new List<CityDto> { new(0, 1.0, 2.0), new(1, 3.0, 4.0) });

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Name_Fails()
    {
        var request = new CreateProblemDefinitionRequest(
            "", null,
            new List<CityDto> { new(0, 1.0, 2.0), new(1, 3.0, 4.0) });

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Single_City_Fails()
    {
        var request = new CreateProblemDefinitionRequest(
            "Test", null,
            new List<CityDto> { new(0, 1.0, 2.0) });

        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Empty_Cities_Fails()
    {
        var request = new CreateProblemDefinitionRequest(
            "Test", null, new List<CityDto>());

        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError();
    }
}
