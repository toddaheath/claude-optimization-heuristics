using FluentAssertions;
using NSubstitute;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Core.Tests.Services;

public class AlgorithmConfigurationServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<AlgorithmConfiguration> _repo;
    private readonly IRepository<OptimizationRun> _runRepo;
    private readonly AlgorithmConfigurationService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public AlgorithmConfigurationServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repo = Substitute.For<IRepository<AlgorithmConfiguration>>();
        _runRepo = Substitute.For<IRepository<OptimizationRun>>();
        _unitOfWork.Repository<AlgorithmConfiguration>().Returns(_repo);
        _unitOfWork.Repository<OptimizationRun>().Returns(_runRepo);
        _service = new AlgorithmConfigurationService(_unitOfWork);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedConfigs()
    {
        var configs = new List<AlgorithmConfiguration>
        {
            new() { Id = Guid.NewGuid(), Name = "SA Config", AlgorithmType = AlgorithmType.SimulatedAnnealing }
        };
        _repo.FindPagedAsync(
            Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>(),
            Arg.Any<Expression<Func<AlgorithmConfiguration, DateTime>>>(),
            1, 50, true).Returns(configs);
        _repo.CountAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns(3);

        var result = await _service.GetAllAsync(_userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var config = new AlgorithmConfiguration { Id = id, Name = "Test" };
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns(config);

        var result = await _service.GetByIdAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SetsIdAndSaves()
    {
        var config = new AlgorithmConfiguration
        {
            Name = "New Config",
            AlgorithmType = AlgorithmType.GeneticAlgorithm,
            MaxIterations = 100,
            Parameters = new Dictionary<string, object>()
        };

        var result = await _service.CreateAsync(config, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.UserId.Should().Be(_userId);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_Existing_UpdatesFields()
    {
        var id = Guid.NewGuid();
        var existing = new AlgorithmConfiguration
        {
            Id = id, Name = "Old", AlgorithmType = AlgorithmType.SimulatedAnnealing,
            MaxIterations = 50, Parameters = new Dictionary<string, object>()
        };
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns(existing);

        var update = new AlgorithmConfiguration
        {
            Name = "New Name", AlgorithmType = AlgorithmType.GeneticAlgorithm,
            MaxIterations = 200, Parameters = new Dictionary<string, object> { { "key", "value" } }
        };

        var result = await _service.UpdateAsync(id, update, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.MaxIterations.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFail()
    {
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new AlgorithmConfiguration(), _userId);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var config = new AlgorithmConfiguration { Id = id, Name = "Test" };
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns(config);
        _runRepo.FindAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>()).Returns(new List<OptimizationRun>());

        var result = await _service.DeleteAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Delete(config);
    }

    [Fact]
    public async Task DeleteAsync_ReferencedByRuns_ReturnsFail()
    {
        var id = Guid.NewGuid();
        var config = new AlgorithmConfiguration { Id = id, Name = "Test" };
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns(config);
        _runRepo.FindAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>())
            .Returns(new List<OptimizationRun> { new() { Id = Guid.NewGuid() } });

        var result = await _service.DeleteAsync(id, _userId);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("referenced by one or more optimization runs");
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _repo.FindOneAsync(Arg.Any<Expression<Func<AlgorithmConfiguration, bool>>>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }
}
