using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IAlgorithmConfigurationService
{
    Task<Result<List<AlgorithmConfiguration>>> GetAllAsync(Guid userId);
    Task<Result<AlgorithmConfiguration>> GetByIdAsync(Guid id, Guid userId);
    Task<Result<AlgorithmConfiguration>> CreateAsync(AlgorithmConfiguration config, Guid userId);
    Task<Result<AlgorithmConfiguration>> UpdateAsync(Guid id, AlgorithmConfiguration config, Guid userId);
    Task<Result> DeleteAsync(Guid id, Guid userId);
}
