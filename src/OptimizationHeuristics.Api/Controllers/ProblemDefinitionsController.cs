using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Extensions;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Controllers;

[ApiController]
[Route("api/v1/problem-definitions")]
public class ProblemDefinitionsController : ControllerBase
{
    private readonly IProblemDefinitionService _service;

    public ProblemDefinitionsController(IProblemDefinitionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.Map(problems => problems.Select(MapToResponse).ToList()).ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProblemDefinitionRequest request)
    {
        var entity = new ProblemDefinition
        {
            Name = request.Name,
            Description = request.Description,
            Cities = request.Cities.Select(c => c.ToModel()).ToList()
        };

        var result = await _service.CreateAsync(entity);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToActionResult();
    }

    private static ProblemDefinitionResponse MapToResponse(ProblemDefinition p) =>
        new(p.Id, p.Name, p.Description, p.Cities.Select(CityDto.FromModel).ToList(),
            p.CityCount, p.CreatedAt, p.UpdatedAt);
}
