using FluentValidation;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Validators;

public class CreateAlgorithmConfigurationValidator : AbstractValidator<CreateAlgorithmConfigurationRequest>
{
    public CreateAlgorithmConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AlgorithmType).IsInEnum();
        RuleFor(x => x.MaxIterations).GreaterThan(0).LessThanOrEqualTo(100000);
        RuleFor(x => x.Parameters).NotNull();
    }
}

public class UpdateAlgorithmConfigurationValidator : AbstractValidator<UpdateAlgorithmConfigurationRequest>
{
    public UpdateAlgorithmConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AlgorithmType).IsInEnum();
        RuleFor(x => x.MaxIterations).GreaterThan(0).LessThanOrEqualTo(100000);
        RuleFor(x => x.Parameters).NotNull();
    }
}
