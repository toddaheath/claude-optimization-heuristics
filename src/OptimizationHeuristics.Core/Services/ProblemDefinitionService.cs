using FluentResults;
using OptimizationHeuristics.Core.Entities;

namespace OptimizationHeuristics.Core.Services;

public class ProblemDefinitionService : IProblemDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProblemDefinitionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ProblemDefinition>>> GetAllAsync()
    {
        var problems = await _unitOfWork.Repository<ProblemDefinition>().GetAllAsync();
        return Result.Ok(problems);
    }

    public async Task<Result<ProblemDefinition>> GetByIdAsync(Guid id)
    {
        var problem = await _unitOfWork.Repository<ProblemDefinition>().GetByIdAsync(id);
        if (problem is null)
            return Result.Fail<ProblemDefinition>("Problem definition not found");
        return Result.Ok(problem);
    }

    public async Task<Result<ProblemDefinition>> CreateAsync(ProblemDefinition problem)
    {
        problem.Id = Guid.NewGuid();
        problem.CityCount = problem.Cities.Count;
        await _unitOfWork.Repository<ProblemDefinition>().AddAsync(problem);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(problem);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var problem = await _unitOfWork.Repository<ProblemDefinition>().GetByIdAsync(id);
        if (problem is null)
            return Result.Fail("Problem definition not found");

        _unitOfWork.Repository<ProblemDefinition>().Delete(problem);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
