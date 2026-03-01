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
    private readonly IRepository<OptimizationRun> _runRepo;
    private readonly ProblemDefinitionService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public ProblemDefinitionServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _repo = Substitute.For<IRepository<ProblemDefinition>>();
        _runRepo = Substitute.For<IRepository<OptimizationRun>>();
        _unitOfWork.Repository<ProblemDefinition>().Returns(_repo);
        _unitOfWork.Repository<OptimizationRun>().Returns(_runRepo);
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
        _repo.FindAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns(problems);

        var result = await _service.GetAllAsync(_userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var problem = new ProblemDefinition { Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0 };
        _repo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns(problem);

        var result = await _service.GetByIdAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _repo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns((ProblemDefinition?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);

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

        var result = await _service.CreateAsync(problem, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CityCount.Should().Be(2);
        result.Value.UserId.Should().Be(_userId);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var problem = new ProblemDefinition { Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0 };
        _repo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns(problem);
        _runRepo.FindAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>()).Returns(new List<OptimizationRun>());

        var result = await _service.DeleteAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Delete(problem);
    }

    [Fact]
    public async Task DeleteAsync_ReferencedByRuns_ReturnsFail()
    {
        var id = Guid.NewGuid();
        var problem = new ProblemDefinition { Id = id, Name = "Test", Cities = new List<City>(), CityCount = 0 };
        _repo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns(problem);
        _runRepo.FindAsync(Arg.Any<Expression<Func<OptimizationRun, bool>>>())
            .Returns(new List<OptimizationRun> { new() { Id = Guid.NewGuid() } });

        var result = await _service.DeleteAsync(id, _userId);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("referenced by one or more optimization runs");
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _repo.FindOneAsync(Arg.Any<Expression<Func<ProblemDefinition, bool>>>()).Returns((ProblemDefinition?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId);

        result.IsFailed.Should().BeTrue();
    }
}
