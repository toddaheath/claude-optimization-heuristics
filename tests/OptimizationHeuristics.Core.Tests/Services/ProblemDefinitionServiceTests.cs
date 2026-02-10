using FluentAssertions;
using NSubstitute;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Core.Services;

namespace OptimizationHeuristics.Core.Tests.Services;

public class ProblemDefinitionServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProblemDefinition> _repo;
    private readonly ProblemDefinitionService _service;

    public ProblemDefinitionServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repo = Substitute.For<IRepository<ProblemDefinition>>();
        _unitOfWork.Repository<ProblemDefinition>().Returns(_repo);
        _service = new ProblemDefinitionService(_unitOfWork);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProblems()
    {
        var problems = new List<ProblemDefinition>
        {
            new() { Id = Guid.NewGuid(), Name = "P1", Cities = new List<City>(), CityCount = 0 },
            new() { Id = Guid.NewGuid(), Name = "P2", Cities = new List<City>(), CityCount = 0 }
        };
        _repo.GetAllAsync().Returns(problems);

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var problem = new ProblemDefinition { Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0 };
        _repo.GetByIdAsync(id).Returns(problem);

        var result = await _service.GetByIdAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((ProblemDefinition?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SetsIdAndCityCount()
    {
        var problem = new ProblemDefinition
        {
            Name = "New Problem",
            Cities = new List<City> { new City(0, 1.0, 2.0), new City(1, 3.0, 4.0) }
        };

        var result = await _service.CreateAsync(problem);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CityCount.Should().Be(2);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var problem = new ProblemDefinition { Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0 };
        _repo.GetByIdAsync(id).Returns(problem);

        var result = await _service.DeleteAsync(id);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Delete(problem);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((ProblemDefinition?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.IsFailed.Should().BeTrue();
    }
}
