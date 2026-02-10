using FluentAssertions;
using NSubstitute;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Core.Tests.Services;

public class OptimizationServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<AlgorithmConfiguration> _configRepo;
    private readonly IRepository<ProblemDefinition> _problemRepo;
    private readonly IRepository<OptimizationRun> _runRepo;
    private readonly OptimizationService _service;

    public OptimizationServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _configRepo = Substitute.For<IRepository<AlgorithmConfiguration>>();
        _problemRepo = Substitute.For<IRepository<ProblemDefinition>>();
        _runRepo = Substitute.For<IRepository<OptimizationRun>>();

        _unitOfWork.Repository<AlgorithmConfiguration>().Returns(_configRepo);
        _unitOfWork.Repository<ProblemDefinition>().Returns(_problemRepo);
        _unitOfWork.Repository<OptimizationRun>().Returns(_runRepo);

        _service = new OptimizationService(_unitOfWork);
    }

    [Fact]
    public async Task RunAsync_ValidInputs_ReturnsCompletedRun()
    {
        var configId = Guid.NewGuid();
        var problemId = Guid.NewGuid();

        _configRepo.GetByIdAsync(configId).Returns(new AlgorithmConfiguration
        {
            Id = configId,
            Name = "SA",
            AlgorithmType = AlgorithmType.SimulatedAnnealing,
            MaxIterations = 50,
            Parameters = new Dictionary<string, object>
            {
                { "initialTemperature", 1000.0 },
                { "coolingRate", 0.99 }
            }
        });

        _problemRepo.GetByIdAsync(problemId).Returns(new ProblemDefinition
        {
            Id = problemId,
            Name = "Test",
            Cities = new List<City>
            {
                new City(0, 0.0, 0.0),
                new City(1, 1.0, 0.0),
                new City(2, 1.0, 1.0),
                new City(3, 0.0, 1.0)
            },
            CityCount = 4
        });

        var result = await _service.RunAsync(configId, problemId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RunStatus.Completed);
        result.Value.BestDistance.Should().BeGreaterThan(0);
        result.Value.BestRoute.Should().HaveCount(4);
        result.Value.IterationHistory.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RunAsync_ConfigNotFound_ReturnsFail()
    {
        _configRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.RunAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_ProblemNotFound_ReturnsFail()
    {
        _configRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new AlgorithmConfiguration
        {
            Id = Guid.NewGuid(), Name = "Test",
            AlgorithmType = AlgorithmType.SimulatedAnnealing,
            MaxIterations = 10, Parameters = new Dictionary<string, object>()
        });
        _problemRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((ProblemDefinition?)null);

        var result = await _service.RunAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedRuns()
    {
        var runs = Enumerable.Range(0, 5).Select(i => new OptimizationRun
        {
            Id = Guid.NewGuid(), Status = RunStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();
        _runRepo.GetAllAsync().Returns(runs);

        var result = await _service.GetAllAsync(1, 3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        _runRepo.GetByIdAsync(id).Returns(new OptimizationRun { Id = id, Status = RunStatus.Completed });

        var result = await _service.GetByIdAsync(id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _runRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((OptimizationRun?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var run = new OptimizationRun { Id = id };
        _runRepo.GetByIdAsync(id).Returns(run);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeTrue();
        _runRepo.Received(1).Delete(run);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _runRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((OptimizationRun?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }
}
