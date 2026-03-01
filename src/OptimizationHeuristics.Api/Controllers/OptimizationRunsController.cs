using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Extensions;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/optimization-runs")]
public class OptimizationRunsController : ControllerBase
{
    private readonly IOptimizationService _service;
    private readonly ICurrentUserService _currentUser;

    public OptimizationRunsController(IOptimizationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<ActionResult> Run([FromBody] RunOptimizationRequest request)
    {
        var result = await _service.RunAsync(
            request.AlgorithmConfigurationId, request.ProblemDefinitionId, _currentUser.UserId);
        return result.Map(MapToResponse).ToCreatedResult();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult> GetProgress(Guid id)
    {
        var result = await _service.GetProgressAsync(id, _currentUser.UserId);
        return result.Map(snap => new RunProgressResponse(
            snap.RunId, snap.Status, snap.IterationHistory,
            snap.BestDistance, snap.ExecutionTimeMs, snap.ErrorMessage
        )).ToActionResult();
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await _service.GetAllAsync(_currentUser.UserId, page, pageSize);
        return result.Map(r => new { items = r.Items.Select(MapToResponse).ToList(), totalCount = r.TotalCount }).ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, _currentUser.UserId);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, _currentUser.UserId);
        return result.ToActionResult();
    }

    private static OptimizationRunResponse MapToResponse(OptimizationRun r) =>
        new(r.Id, r.AlgorithmConfigurationId, r.ProblemDefinitionId, r.Status,
            r.BestDistance, r.BestRoute, r.IterationHistory, r.TotalIterations,
            r.ExecutionTimeMs, r.CreatedAt, r.UpdatedAt);
}
