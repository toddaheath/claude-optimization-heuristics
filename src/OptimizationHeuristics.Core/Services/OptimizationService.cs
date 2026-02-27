using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OptimizationHeuristics.Core.Algorithms;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Errors;

namespace OptimizationHeuristics.Core.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRunProgressStore _progressStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OptimizationService> _logger;

    public OptimizationService(IUnitOfWork unitOfWork, IRunProgressStore progressStore, IServiceScopeFactory scopeFactory, ILogger<OptimizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _progressStore = progressStore;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<Result<OptimizationRun>> RunAsync(Guid configurationId, Guid problemId, Guid userId)
    {
        var config = await _unitOfWork.Repository<AlgorithmConfiguration>().FindOneAsync(x => x.Id == configurationId && x.UserId == userId);
        if (config is null)
            return Result.Fail<OptimizationRun>(new NotFoundError("Algorithm configuration not found"));

        var problem = await _unitOfWork.Repository<ProblemDefinition>().FindOneAsync(x => x.Id == problemId && x.UserId == userId);
        if (problem is null)
            return Result.Fail<OptimizationRun>(new NotFoundError("Problem definition not found"));

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

        var cancellationToken = _progressStore.GetCancellationToken(runId);
        _ = Task.Run(async () => await ExecuteRunAsync(runId, algorithmType, maxIterations, parameters, cities, cancellationToken));

        return Result.Ok(run);
    }

    private async Task ExecuteRunAsync(
        Guid runId,
        Core.Enums.AlgorithmType algorithmType,
        int maxIterations,
        Dictionary<string, object> parameters,
        IReadOnlyList<Core.Models.City> cities,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var run = await unitOfWork.Repository<OptimizationRun>().GetByIdAsync(runId);
        if (run is null) return;

        try
        {
            var algorithm = AlgorithmFactory.Create(algorithmType);
            var result = algorithm.Solve(cities, maxIterations, parameters,
                iterResult => _progressStore.AddIteration(runId, iterResult), cancellationToken);

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
            _logger.LogError(ex, "Optimization run {RunId} failed with {ExceptionType}: {Message}", runId, ex.GetType().Name, ex.Message);
            run.Status = RunStatus.Failed;
            _progressStore.FailRun(runId, "An error occurred during optimization");
        }

        unitOfWork.Repository<OptimizationRun>().Update(run);
        await unitOfWork.SaveChangesAsync();
        _progressStore.CleanUp(runId);
    }

    public async Task<Result<RunProgressSnapshot>> GetProgressAsync(Guid runId, Guid userId)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().FindOneAsync(x => x.Id == runId && x.UserId == userId);
        if (run is null)
            return Result.Fail<RunProgressSnapshot>(new NotFoundError("Optimization run not found"));

        var snapshot = _progressStore.GetSnapshot(runId);
        return snapshot is null
            ? Result.Fail<RunProgressSnapshot>("Run not found in progress store")
            : Result.Ok(snapshot);
    }

    public async Task<Result<List<OptimizationRun>>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var runs = await _unitOfWork.Repository<OptimizationRun>()
            .FindPagedAsync(x => x.UserId == userId, x => x.CreatedAt, page, pageSize, descending: true);
        return Result.Ok(runs);
    }

    public async Task<Result<OptimizationRun>> GetByIdAsync(Guid id, Guid userId)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (run is null)
            return Result.Fail<OptimizationRun>(new NotFoundError("Optimization run not found"));
        return Result.Ok(run);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId)
    {
        var run = await _unitOfWork.Repository<OptimizationRun>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (run is null)
            return Result.Fail(new NotFoundError("Optimization run not found"));

        _progressStore.CancelRun(id);
        _unitOfWork.Repository<OptimizationRun>().Delete(run);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
