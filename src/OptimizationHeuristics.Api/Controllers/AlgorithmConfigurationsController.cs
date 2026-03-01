using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Extensions;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/algorithm-configurations")]
public class AlgorithmConfigurationsController : ControllerBase
{
    private readonly IAlgorithmConfigurationService _service;
    private readonly ICurrentUserService _currentUser;

    public AlgorithmConfigurationsController(IAlgorithmConfigurationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
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

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAlgorithmConfigurationRequest request)
    {
        var entity = new AlgorithmConfiguration
        {
            Name = request.Name,
            Description = request.Description,
            AlgorithmType = request.AlgorithmType,
            Parameters = request.Parameters,
            MaxIterations = request.MaxIterations
        };

        var result = await _service.CreateAsync(entity, _currentUser.UserId);
        return result.Map(MapToResponse).ToCreatedResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateAlgorithmConfigurationRequest request)
    {
        var entity = new AlgorithmConfiguration
        {
            Name = request.Name,
            Description = request.Description,
            AlgorithmType = request.AlgorithmType,
            Parameters = request.Parameters,
            MaxIterations = request.MaxIterations
        };

        var result = await _service.UpdateAsync(id, entity, _currentUser.UserId);
        return result.Map(MapToResponse).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, _currentUser.UserId);
        return result.ToActionResult();
    }

    private static AlgorithmConfigurationResponse MapToResponse(AlgorithmConfiguration c) =>
        new(c.Id, c.Name, c.Description, c.AlgorithmType, c.Parameters,
            c.MaxIterations, c.CreatedAt, c.UpdatedAt);
}
