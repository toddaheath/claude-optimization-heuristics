using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IProblemDefinitionService
{
    Task<Result<List<ProblemDefinition>>> GetAllAsync(Guid userId);
    Task<Result<ProblemDefinition>> GetByIdAsync(Guid id, Guid userId);
    Task<Result<ProblemDefinition>> CreateAsync(ProblemDefinition problem, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid userId);
}
