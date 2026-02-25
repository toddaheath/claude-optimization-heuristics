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

public class AlgorithmConfigurationsControllerTests
{
    private readonly IAlgorithmConfigurationService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AlgorithmConfigurationsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public AlgorithmConfigurationsControllerTests()
    {
        _service = Substitute.For<IAlgorithmConfigurationService>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(_userId);
        _controller = new AlgorithmConfigurationsController(_service, _currentUser);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _service.GetAllAsync(_userId).Returns(Result.Ok(new List<AlgorithmConfiguration>
        {
            new() { Id = Guid.NewGuid(), Name = "Config", AlgorithmType = AlgorithmType.SimulatedAnnealing,
                     Parameters = new Dictionary<string, object>() }
        }));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOk()
    {
        _service.CreateAsync(Arg.Any<AlgorithmConfiguration>(), _userId).Returns(ci =>
        {
            var config = ci.Arg<AlgorithmConfiguration>();
            config.Id = Guid.NewGuid();
            return Result.Ok(config);
        });

        var request = new CreateAlgorithmConfigurationRequest(
            "SA Config", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 100);

        var result = await _controller.Create(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.UpdateAsync(id, Arg.Any<AlgorithmConfiguration>(), _userId).Returns(Result.Ok(new AlgorithmConfiguration
        {
            Id = id, Name = "Updated", AlgorithmType = AlgorithmType.GeneticAlgorithm,
            Parameters = new Dictionary<string, object>(), MaxIterations = 200
        }));

        var request = new UpdateAlgorithmConfigurationRequest(
            "Updated", null, AlgorithmType.GeneticAlgorithm,
            new Dictionary<string, object>(), 200);

        var result = await _controller.Update(id, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _service.DeleteAsync(Arg.Any<Guid>(), _userId).Returns(Result.Ok());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}
