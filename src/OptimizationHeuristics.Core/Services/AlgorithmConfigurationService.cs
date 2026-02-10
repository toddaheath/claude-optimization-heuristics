using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public class AlgorithmConfigurationService : IAlgorithmConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;

    public AlgorithmConfigurationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AlgorithmConfiguration>>> GetAllAsync()
    {
        var configs = await _unitOfWork.Repository<AlgorithmConfiguration>().GetAllAsync();
        return Result.Ok(configs);
    }

    public async Task<Result<AlgorithmConfiguration>> GetByIdAsync(Guid id)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().GetByIdAsync(id);
        if (config is null)
            return Result.Fail<AlgorithmConfiguration>("Algorithm configuration not found");
        return Result.Ok(config);
    }

    public async Task<Result<AlgorithmConfiguration>> CreateAsync(AlgorithmConfiguration config)
    {
        config.Id = Guid.NewGuid();
        await _unitOfWork.Repository<AlgorithmConfiguration>().AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(config);
    }

    public async Task<Result<AlgorithmConfiguration>> UpdateAsync(Guid id, AlgorithmConfiguration config)
    {
        var existing = await _unitOfWork.Repository<AlgorithmConfiguration>().GetByIdAsync(id);
        if (existing is null)
            return Result.Fail<AlgorithmConfiguration>("Algorithm configuration not found");

        existing.Name = config.Name;
        existing.Description = config.Description;
        existing.AlgorithmType = config.AlgorithmType;
        existing.Parameters = config.Parameters;
        existing.MaxIterations = config.MaxIterations;

        _unitOfWork.Repository<AlgorithmConfiguration>().Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(existing);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().GetByIdAsync(id);
        if (config is null)
            return Result.Fail("Algorithm configuration not found");

        _unitOfWork.Repository<AlgorithmConfiguration>().Delete(config);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
