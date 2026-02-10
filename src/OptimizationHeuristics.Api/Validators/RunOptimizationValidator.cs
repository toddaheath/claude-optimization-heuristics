using FluentValidation;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Validators;

public class RunOptimizationValidator : AbstractValidator<RunOptimizationRequest>
{
    public RunOptimizationValidator()
    {
        RuleFor(x => x.AlgorithmConfigurationId).NotEmpty();
        RuleFor(x => x.ProblemDefinitionId).NotEmpty();
    }
}
