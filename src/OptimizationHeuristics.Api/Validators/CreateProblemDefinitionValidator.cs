using FluentValidation;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Validators;

public class CreateProblemDefinitionValidator : AbstractValidator<CreateProblemDefinitionRequest>
{
    public CreateProblemDefinitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cities).NotEmpty().WithMessage("At least one city is required");
        RuleFor(x => x.Cities.Count).GreaterThanOrEqualTo(2).WithMessage("At least 2 cities are required");
        RuleForEach(x => x.Cities).ChildRules(city =>
        {
            city.RuleFor(c => c.X).InclusiveBetween(-10000, 10000);
            city.RuleFor(c => c.Y).InclusiveBetween(-10000, 10000);
        });
    }
}
