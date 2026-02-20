using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Extensions;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Controllers;

[ApiController]
[Route("api/v1/optimization-runs")]
public class OptimizationRunsController : ControllerBase
{
    private readonly IOptimizationService _service;

    public OptimizationRunsController(IOptimizationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult> Run([FromBody] RunOptimizationRequest request)
    {
        var result = await _service.RunAsync(request.AlgorithmConfigurationId, request.ProblemDefinitionId);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult> GetProgress(Guid id)
    {
        var result = await _service.GetProgressAsync(id);
        return result.Map(snap => new RunProgressResponse(
            snap.RunId, snap.Status, snap.IterationHistory,
            snap.BestDistance, snap.ExecutionTimeMs, snap.ErrorMessage
        )).ToActionResult();
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return result.Map(runs => runs.Select(MapToResponse).ToList()).ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToActionResult();
    }

    private static OptimizationRunResponse MapToResponse(OptimizationRun r) =>
        new(r.Id, r.AlgorithmConfigurationId, r.ProblemDefinitionId, r.Status,
            r.BestDistance, r.BestRoute, r.IterationHistory, r.TotalIterations,
            r.ExecutionTimeMs, r.CreatedAt, r.UpdatedAt);
}
