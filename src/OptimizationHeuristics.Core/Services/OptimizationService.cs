using FluentResults;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IUnitOfWork _unitOfWork;

    public OptimizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OptimizationRun>> RunAsync(Guid configurationId, Guid problemId)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().GetByIdAsync(configurationId);
        if (config is null)
            return Result.Fail<OptimizationRun>("Algorithm configuration not found");

        var problem = await _unitOfWork.Repository<ProblemDefinition>().GetByIdAsync(problemId);
        if (problem is null)
            return Result.Fail<OptimizationRun>("Problem definition not found");

        var run = new OptimizationRun
        {
            Id = Guid.NewGuid(),
            AlgorithmConfigurationId = configurationId,
            ProblemDefinitionId = problemId,
            Status = RunStatus.Running
        };

        await _unitOfWork.Repository<OptimizationRun>().AddAsync(run);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            var algorithm = AlgorithmFactory.Create(config.AlgorithmType);
            var result = algorithm.Solve(problem.Cities, config.MaxIterations, config.Parameters);

            run.Status = RunStatus.Completed;
            run.BestDistance = result.BestDistance;
            run.BestRoute = result.BestRoute;
            run.IterationHistory = result.IterationHistory;
            run.TotalIterations = result.TotalIterations;
            run.ExecutionTimeMs = result.ExecutionTimeMs;
        }
        catch (Exception)
        {
            run.Status = RunStatus.Failed;
        }

        _unitOfWork.Repository<OptimizationRun>().Update(run);
        await _unitOfWork.SaveChangesAsync();

        return Result.Ok(run);
    }

    public async Task<Result<List<OptimizationRun>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var runs = await _unitOfWork.Repository<OptimizationRun>().GetAllAsync();
        var paged = runs
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Result.Ok(paged);
    }

    public async Task<Result<OptimizationRun>> GetByIdAsync(Guid id)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().GetByIdAsync(id);
        if (run is null)
            return Result.Fail<OptimizationRun>("Optimization run not found");
        return Result.Ok(run);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().GetByIdAsync(id);
        if (run is null)
            return Result.Fail("Optimization run not found");

        _unitOfWork.Repository<OptimizationRun>().Delete(run);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
