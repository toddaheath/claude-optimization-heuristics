using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OptimizationHeuristics.Api.Controllers;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Tests.Controllers;

public class OptimizationRunsControllerTests
{
    private readonly IOptimizationService _service;
    private readonly OptimizationRunsController _controller;

    public OptimizationRunsControllerTests()
    {
        _service = Substitute.For<IOptimizationService>();
        _controller = new OptimizationRunsController(_service);
    }

    [Fact]
    public async Task Run_ValidRequest_ReturnsOk()
    {
        _service.RunAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Result.Ok(new OptimizationRun
        {
            Id = Guid.NewGuid(), Status = RunStatus.Completed,
            BestDistance = 100.0, BestRoute = new List<int> { 0, 1, 2 },
            TotalIterations = 50
        }));

        var result = await _controller.Run(new RunOptimizationRequest(Guid.NewGuid(), Guid.NewGuid()));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _service.GetAllAsync(1, 20).Returns(Result.Ok(new List<OptimizationRun>()));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.GetByIdAsync(id).Returns(Result.Ok(new OptimizationRun
        {
            Id = id, Status = RunStatus.Completed
        }));

        var result = await _controller.GetById(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>()).Returns(Result.Fail<OptimizationRun>("not found"));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _service.DeleteAsync(Arg.Any<Guid>()).Returns(Result.Ok());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}
