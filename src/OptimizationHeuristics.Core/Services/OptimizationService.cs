using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRunProgressStore _progressStore;
    private readonly IServiceScopeFactory _scopeFactory;

    public OptimizationService(IUnitOfWork unitOfWork, IRunProgressStore progressStore, IServiceScopeFactory scopeFactory)
    {
        _unitOfWork = unitOfWork;
        _progressStore = progressStore;
        _scopeFactory = scopeFactory;
    }

    public async Task<Result<OptimizationRun>> RunAsync(Guid configurationId, Guid problemId, Guid userId)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().FindOneAsync(x => x.Id == configurationId && x.UserId == userId);
        if (config is null)
            return Result.Fail<OptimizationRun>("Algorithm configuration not found");

        var problem = await _unitOfWork.Repository<ProblemDefinition>().FindOneAsync(x => x.Id == problemId && x.UserId == userId);
        if (problem is null)
            return Result.Fail<OptimizationRun>("Problem definition not found");

        var run = new OptimizationRun
        {
            Id = Guid.NewGuid(),
            AlgorithmConfigurationId = configurationId,
            ProblemDefinitionId = problemId,
            UserId = userId,
            Status = RunStatus.Running
        };

        await _unitOfWork.Repository<OptimizationRun>().AddAsync(run);
        await _unitOfWork.SaveChangesAsync();

        _progressStore.InitRun(run.Id);

        // Capture values for background task
        var runId = run.Id;
        var algorithmType = config.AlgorithmType;
        var maxIterations = config.MaxIterations;
        var parameters = new Dictionary<string, object>(config.Parameters);
        var cities = problem.Cities;

        _ = Task.Run(async () => await ExecuteRunAsync(runId, algorithmType, maxIterations, parameters, cities));

        return Result.Ok(run);
    }

    private async Task ExecuteRunAsync(
        Guid runId,
        Core.Enums.AlgorithmType algorithmType,
        int maxIterations,
        Dictionary<string, object> parameters,
        IReadOnlyList<Core.Models.City> cities)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var run = await unitOfWork.Repository<OptimizationRun>().GetByIdAsync(runId);
        if (run is null) return;

        try
        {
            var algorithm = AlgorithmFactory.Create(algorithmType);
            var result = algorithm.Solve(cities, maxIterations, parameters,
                iterResult => _progressStore.AddIteration(runId, iterResult));

            run.Status = RunStatus.Completed;
            run.BestDistance = result.BestDistance;
            run.BestRoute = result.BestRoute;
            run.IterationHistory = result.IterationHistory;
            run.TotalIterations = result.TotalIterations;
            run.ExecutionTimeMs = result.ExecutionTimeMs;

            _progressStore.CompleteRun(runId, result.BestDistance, result.ExecutionTimeMs);
        }
        catch (Exception ex)
        {
            run.Status = RunStatus.Failed;
            _progressStore.FailRun(runId, ex.Message);
        }

        unitOfWork.Repository<OptimizationRun>().Update(run);
        await unitOfWork.SaveChangesAsync();
    }

    public Task<Result<RunProgressSnapshot>> GetProgressAsync(Guid runId)
    {
        var snapshot = _progressStore.GetSnapshot(runId);
        return snapshot is null
            ? Task.FromResult(Result.Fail<RunProgressSnapshot>("Run not found in progress store"))
            : Task.FromResult(Result.Ok(snapshot));
    }

    public async Task<Result<List<OptimizationRun>>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var runs = await _unitOfWork.Repository<OptimizationRun>().FindAsync(x => x.UserId == userId);
        var paged = runs
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Result.Ok(paged);
    }

    public async Task<Result<OptimizationRun>> GetByIdAsync(Guid id, Guid userId)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (run is null)
            return Result.Fail<OptimizationRun>("Optimization run not found");
        return Result.Ok(run);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (run is null)
            return Result.Fail("Optimization run not found");

        _unitOfWork.Repository<OptimizationRun>().Delete(run);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
