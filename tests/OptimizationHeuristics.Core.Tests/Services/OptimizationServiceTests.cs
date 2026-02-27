using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly IRunProgressStore _progressStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OptimizationService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public OptimizationServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _configRepo = Substitute.For<IRepository<AlgorithmConfiguration>>();
        _problemRepo = Substitute.For<IRepository<ProblemDefinition>>();
        _runRepo = Substitute.For<IRepository<OptimizationRun>>();
        _progressStore = Substitute.For<IRunProgressStore>();

        _unitOfWork.Repository<AlgorithmConfiguration>().Returns(_configRepo);
        _unitOfWork.Repository<ProblemDefinition>().Returns(_problemRepo);
        _unitOfWork.Repository<OptimizationRun>().Returns(_runRepo);

        // Wire up scope factory so the background task can resolve IUnitOfWork
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var sp = Substitute.For<IServiceProvider>();
        _scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(sp);
        sp.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);

        _service = new OptimizationService(_unitOfWork, _progressStore, _scopeFactory, Substitute.For<ILogger<OptimizationService>>());
    }

    [Fact]
    public async Task RunAsync_ValidInputs_ReturnsRunningStatus()
    {
        var configId = Guid.NewGuid();
        var problemId = Guid.NewGuid();

        _configRepo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>())
            .Returns(new AlgorithmConfiguration
            {
                Id = configId,
                Name = "SA",
                AlgorithmType = AlgorithmType.SimulatedAnnealing,
                MaxIterations = 20,
                Parameters = new Dictionary<string, object>
                {
                    { "initialTemperature", 1000.0 },
                    { "coolingRate", 0.99 }
                }
            });

        _problemRepo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>())
            .Returns(new ProblemDefinition
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

        var result = await _service.RunAsync(configId, problemId, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(RunStatus.Running);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task RunAsync_ConfigNotFound_ReturnsFail()
    {
        _configRepo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>())
            .Returns((AlgorithmConfiguration?)null);

        var result = await _service.RunAsync(Guid.NewGuid(), Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_ProblemNotFound_ReturnsFail()
    {
        _configRepo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>())
            .Returns(new AlgorithmConfiguration
            {
                Id = Guid.NewGuid(), Name = "Test",
                AlgorithmType = AlgorithmType.SimulatedAnnealing,
                MaxIterations = 10, Parameters = new Dictionary<string, object>()
            });
        _problemRepo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>())
            .Returns((ProblemDefinition?)null);

        var result = await _service.RunAsync(Guid.NewGuid(), Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedRuns()
    {
        var runs = Enumerable.Range(0, 3).Select(i => new OptimizationRun
        {
            Id = Guid.NewGuid(), Status = RunStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();
        _runRepo.FindPagedAsync(
            Arg.Any<Expression<Func<OptimizationRun, bool>>>(),
            Arg.Any<Expression<Func<OptimizationRun, DateTime>>>(),
            1, 3, true).Returns(runs);

        var result = await _service.GetAllAsync(_userId, 1, 3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        _runRepo.FindOneAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>())
            .Returns(new OptimizationRun { Id = id, Status = RunStatus.Completed });

        var result = await _service.GetByIdAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _runRepo.FindOneAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>())
            .Returns((OptimizationRun?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var run = new OptimizationRun { Id = id };
        _runRepo.FindOneAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>()).Returns(run);

        var result = await _service.DeleteAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
        _runRepo.Received(1).Delete(run);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _runRepo.FindOneAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>())
            .Returns((OptimizationRun?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetProgressAsync_UnknownRun_ReturnsFail()
    {
        _runRepo.FindOneAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OptimizationRun, bool>>>())
            .Returns((OptimizationRun?)null);
        _progressStore.GetSnapshot(Arg.Any<Guid>()).Returns((RunProgressSnapshot?)null);

        var result = await _service.GetProgressAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetProgressAsync_KnownRun_ReturnsSnapshot()
    {
        var runId = Guid.NewGuid();
        var run = new OptimizationRun { Id = runId, UserId = _userId, Status = RunStatus.Running };
        _runRepo.FindOneAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OptimizationRun, bool>>>())
            .Returns(run);
        var snapshot = new RunProgressSnapshot(runId, RunStatus.Running, [], null, 0, null);
        _progressStore.GetSnapshot(runId).Returns(snapshot);

        var result = await _service.GetProgressAsync(runId, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.RunId.Should().Be(runId);
    }
}
