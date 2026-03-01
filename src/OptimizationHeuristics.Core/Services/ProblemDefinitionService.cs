using FluentResults;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Errors;

namespace OptimizationHeuristics.Core.Services;

public class ProblemDefinitionService : IProblemDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProblemDefinitionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<(List<ProblemDefinition> Items, int TotalCount)>> GetAllAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        var problems = await _unitOfWork.Repository<ProblemDefinition>()
            .FindPagedAsync(x => x.UserId == userId, x => x.CreatedAt, page, pageSize, descending: true);
        var totalCount = await _unitOfWork.Repository<ProblemDefinition>()
            .CountAsync(x => x.UserId == userId);
        return Result.Ok((problems, totalCount));
    }

    public async Task<Result<ProblemDefinition>> GetByIdAsync(Guid id, Guid userId)
    {
        var problem = await _unitOfWork.Repository<ProblemDefinition>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (problem is null)
            return Result.Fail<ProblemDefinition>(new NotFoundError("Problem definition not found"));
        return Result.Ok(problem);
    }

    public async Task<Result<ProblemDefinition>> CreateAsync(ProblemDefinition problem, Guid userId)
    {
        problem.Id = Guid.NewGuid();
        problem.CityCount = problem.Cities.Count;
        problem.UserId = userId;
        await _unitOfWork.Repository<ProblemDefinition>().AddAsync(problem);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(problem);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId)
    {
        var problem = await _unitOfWork.Repository<ProblemDefinition>().FindOneAsync(x => x.Id == id && x.UserId == userId);
        if (problem is null)
            return Result.Fail(new NotFoundError("Problem definition not found"));

        var referencingRuns = await _unitOfWork.Repository<OptimizationRun>().FindAsync(r => r.ProblemDefinitionId == id && r.UserId == userId);
        if (referencingRuns.Count > 0)
            return Result.Fail("Cannot delete problem definition because it is referenced by one or more optimization runs. Delete those runs first.");

        _unitOfWork.Repository<ProblemDefinition>().Delete(problem);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
