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

    public async Task<Result<List<ProblemDefinition>>> GetAllAsync(Guid userId)
    {
        var problems = await _unitOfWork.Repository<ProblemDefinition>().FindAsync(x => x.UserId == userId);
        return Result.Ok(problems);
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
