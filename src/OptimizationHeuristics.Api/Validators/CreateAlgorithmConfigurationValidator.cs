using FluentValidation;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Validators;

public class CreateAlgorithmConfigurationValidator : AbstractValidator<CreateAlgorithmConfigurationRequest>
{
    public CreateAlgorithmConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200)
            .WithMessage("Name is required and must be at most 200 characters.");
        RuleFor(x => x.Description).MaximumLength(1000)
            .WithMessage("Description must be at most 1000 characters.");
        RuleFor(x => x.AlgorithmType).IsInEnum()
            .WithMessage("Invalid algorithm type.");
        RuleFor(x => x.MaxIterations).GreaterThan(0).LessThanOrEqualTo(100000)
            .WithMessage("Max iterations must be between 1 and 100,000.");
        RuleFor(x => x.Parameters).NotNull()
            .WithMessage("Parameters dictionary is required.");
    }
}

public class UpdateAlgorithmConfigurationValidator : AbstractValidator<UpdateAlgorithmConfigurationRequest>
{
    public UpdateAlgorithmConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200)
            .WithMessage("Name is required and must be at most 200 characters.");
        RuleFor(x => x.Description).MaximumLength(1000)
            .WithMessage("Description must be at most 1000 characters.");
        RuleFor(x => x.AlgorithmType).IsInEnum()
            .WithMessage("Invalid algorithm type.");
        RuleFor(x => x.MaxIterations).GreaterThan(0).LessThanOrEqualTo(100000)
            .WithMessage("Max iterations must be between 1 and 100,000.");
        RuleFor(x => x.Parameters).NotNull()
            .WithMessage("Parameters dictionary is required.");
    }
}
