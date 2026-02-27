using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OptimizationHeuristics.Api.Controllers;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Errors;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Tests.Controllers;

public class OptimizationRunsControllerTests
{
    private readonly IOptimizationService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly OptimizationRunsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public OptimizationRunsControllerTests()
    {
        _service = Substitute.For<IOptimizationService>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(_userId);
        _controller = new OptimizationRunsController(_service, _currentUser);
    }

    [Fact]
    public async Task Run_ValidRequest_ReturnsOk()
    {
        _service.RunAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), _userId).Returns(Result.Ok(new OptimizationRun
        {
            Id = Guid.NewGuid(), Status = RunStatus.Completed,
            BestDistance = 100.0, BestRoute = new List<int> { 0, 1, 2 },
            TotalIterations = 50
        }));

        var result = await _controller.Run(new RunOptimizationRequest(Guid.NewGuid(), Guid.NewGuid()));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _service.GetAllAsync(_userId, 1, 20).Returns(Result.Ok(new List<OptimizationRun>()));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.GetByIdAsync(id, _userId).Returns(Result.Ok(new OptimizationRun
        {
            Id = id, Status = RunStatus.Completed
        }));

        var result = await _controller.GetById(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), _userId).Returns(Result.Fail<OptimizationRun>(new NotFoundError("not found")));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _service.DeleteAsync(Arg.Any<Guid>(), _userId).Returns(Result.Ok());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetProgress_ValidId_ReturnsOk()
    {
        var runId = Guid.NewGuid();
        var snapshot = new RunProgressSnapshot(runId, RunStatus.Running, [], null, 0, null);
        _service.GetProgressAsync(runId, _userId).Returns(Result.Ok(snapshot));

        var result = await _controller.GetProgress(runId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProgress_NotFound_ReturnsNotFound()
    {
        _service.GetProgressAsync(Arg.Any<Guid>(), _userId)
            .Returns(Result.Fail<RunProgressSnapshot>(new NotFoundError("not found")));

        var result = await _controller.GetProgress(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
