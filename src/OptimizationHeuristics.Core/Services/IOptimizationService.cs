using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IOptimizationService
{
    Task<Result<OptimizationRun>> RunAsync(Guid configurationId, Guid problemId);
    Task<Result<List<OptimizationRun>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<OptimizationRun>> GetByIdAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}
