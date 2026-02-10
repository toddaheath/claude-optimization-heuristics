using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public interface IAlgorithmConfigurationService
{
    Task<Result<List<AlgorithmConfiguration>>> GetAllAsync();
    Task<Result<AlgorithmConfiguration>> GetByIdAsync(Guid id);
    Task<Result<AlgorithmConfiguration>> CreateAsync(AlgorithmConfiguration config);
    Task<Result<AlgorithmConfiguration>> UpdateAsync(Guid id, AlgorithmConfiguration config);
    Task<Result> DeleteAsync(Guid id);
}
