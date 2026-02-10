using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IProblemDefinitionService
{
    Task<Result<List<ProblemDefinition>>> GetAllAsync();
    Task<Result<ProblemDefinition>> GetByIdAsync(Guid id);
    Task<Result<ProblemDefinition>> CreateAsync(ProblemDefinition problem);
    Task<Result> DeleteAsync(Guid id);
}
