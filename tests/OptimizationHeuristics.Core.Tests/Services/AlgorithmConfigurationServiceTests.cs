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
    private readonly AlgorithmConfigurationService _service;

    public AlgorithmConfigurationServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repo = Substitute.For<IRepository<AlgorithmConfiguration>>();
        _unitOfWork.Repository<AlgorithmConfiguration>().Returns(_repo);
        _service = new AlgorithmConfigurationService(_unitOfWork);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        var configs = new List<AlgorithmConfiguration>
        {
            new() { Id = Guid.NewGuid(), Name = "SA Config", AlgorithmType = AlgorithmType.SimulatedAnnealing }
        };
        _repo.GetAllAsync().Returns(configs);

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var config = new AlgorithmConfiguration { Id = id, Name = "Test" };
        _repo.GetByIdAsync(id).Returns(config);

        var result = await _service.GetByIdAsync(id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

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

        var result = await _service.CreateAsync(config);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
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
        _repo.GetByIdAsync(id).Returns(existing);

        var update = new AlgorithmConfiguration
        {
            Name = "New Name", AlgorithmType = AlgorithmType.GeneticAlgorithm,
            MaxIterations = 200, Parameters = new Dictionary<string, object> { { "key", "value" } }
        };

        var result = await _service.UpdateAsync(id, update);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.MaxIterations.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFail()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new AlgorithmConfiguration());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var config = new AlgorithmConfiguration { Id = id, Name = "Test" };
        _repo.GetByIdAsync(id).Returns(config);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Delete(config);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((AlgorithmConfiguration?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }
}
