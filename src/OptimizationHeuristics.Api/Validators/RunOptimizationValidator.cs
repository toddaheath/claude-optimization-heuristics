using FluentValidation;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Validators;

public class RunOptimizationValidator : AbstractValidator<RunOptimizationRequest>
{
    public RunOptimizationValidator()
    {
        RuleFor(x => x.AlgorithmConfigurationId).NotEmpty()
            .WithMessage("Algorithm configuration ID is required.");
        RuleFor(x => x.ProblemDefinitionId).NotEmpty()
            .WithMessage("Problem definition ID is required.");
    }
}
