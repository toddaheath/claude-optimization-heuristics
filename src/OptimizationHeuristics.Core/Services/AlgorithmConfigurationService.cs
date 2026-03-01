using FluentResults;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Errors;

namespace OptimizationHeuristics.Core.Services;

public class AlgorithmConfigurationService : IAlgorithmConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;

    public AlgorithmConfigurationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AlgorithmConfiguration>>> GetAllAsync(Guid userId)
    {
        var configs = await _unitOfWork.Repository<AlgorithmConfiguration>().FindAsync(x => x.UserId == userId);
        return Result.Ok(configs);
    }

    public async Task<Result<AlgorithmConfiguration>> GetByIdAsync(Guid id, Guid userId)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (config is null)
            return Result.Fail<AlgorithmConfiguration>(new NotFoundError("Algorithm configuration not found"));
        return Result.Ok(config);
    }

    public async Task<Result<AlgorithmConfiguration>> CreateAsync(AlgorithmConfiguration config, Guid userId)
    {
        config.Id = Guid.NewGuid();
        config.UserId = userId;
        await _unitOfWork.Repository<AlgorithmConfiguration>().AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(config);
    }

    public async Task<Result<AlgorithmConfiguration>> UpdateAsync(Guid id, AlgorithmConfiguration config, Guid userId)
    {
        var existing = await _unitOfWork.Repository<AlgorithmConfiguration>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (existing is null)
            return Result.Fail<AlgorithmConfiguration>(new NotFoundError("Algorithm configuration not found"));

        existing.Name = config.Name;
        existing.Description = config.Description;
        existing.AlgorithmType = config.AlgorithmType;
        existing.Parameters = config.Parameters;
        existing.MaxIterations = config.MaxIterations;

        _unitOfWork.Repository<AlgorithmConfiguration>().Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(existing);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (config is null)
            return Result.Fail(new NotFoundError("Algorithm configuration not found"));

        var referencingRuns = await _unitOfWork.Repository<OptimizationRun>().FindAsync(r => r.AlgorithmConfigurationId == id && r.UserId == userId);
        if (referencingRuns.Count > 0)
            return Result.Fail("Cannot delete algorithm configuration because it is referenced by one or more optimization runs. Delete those runs first.");

        _unitOfWork.Repository<AlgorithmConfiguration>().Delete(config);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
