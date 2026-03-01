using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OptimizationHeuristics.Api.Controllers;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Errors;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Api.Tests.Controllers;

public class ProblemDefinitionsControllerTests
{
    private readonly IProblemDefinitionService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ProblemDefinitionsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public ProblemDefinitionsControllerTests()
    {
        _service = Substitute.For<IProblemDefinitionService>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(_userId);
        _controller = new ProblemDefinitionsController(_service, _currentUser);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _service.GetAllAsync(_userId, 1, 50).Returns(Result.Ok((new List<ProblemDefinition>
        {
            new() { Id = Guid.NewGuid(), Name = "P1", Cities = new List<City>(), CityCount = 0 }
        }, 1)));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.GetByIdAsync(id, _userId).Returns(Result.Ok(new ProblemDefinition
        {
            Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0
        }));

        var result = await _controller.GetById(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), _userId).Returns(Result.Fail<ProblemDefinition>(new NotFoundError("not found")));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsOk()
    {
        _service.CreateAsync(Arg.Any<ProblemDefinition>(), _userId).Returns(ci =>
        {
            var pd = ci.Arg<ProblemDefinition>();
            pd.Id = Guid.NewGuid();
            return Result.Ok(pd);
        });

        var request = new CreateProblemDefinitionRequest("Test", null,
            new List<CityDto> { new(0, 1.0, 2.0), new(1, 3.0, 4.0) });

        var result = await _controller.Create(request);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _service.DeleteAsync(Arg.Any<Guid>(), _userId).Returns(Result.Ok());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _service.DeleteAsync(Arg.Any<Guid>(), _userId).Returns(Result.Fail(new NotFoundError("not found")));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
