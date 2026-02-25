using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IOptimizationService
{
    /// <summary>Creates the run record and starts execution in a background task. Returns immediately.</summary>
    Task<Result<OptimizationRun>> RunAsync(Guid configurationId, Guid problemId, Guid userId);
    Task<Result<RunProgressSnapshot>> GetProgressAsync(Guid runId);
    Task<Result<List<OptimizationRun>>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<OptimizationRun>> GetByIdAsync(Guid id, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid userId);
}
