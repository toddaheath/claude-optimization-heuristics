using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Infrastructure.Data;
using OptimizationHeuristics.Infrastructure.Repositories;

namespace OptimizationHeuristics.Infrastructure.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AndGetById_ReturnsSameEntity()
    {
        var repo = new Repository<ProblemDefinition>(_context);
        var problem = new ProblemDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Test Problem",
            Cities = new List<City> { new City(0, 1.0, 2.0), new City(1, 3.0, 4.0) },
            CityCount = 2
        };

        await repo.AddAsync(problem);
        await _context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(problem.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Problem");
        retrieved.CityCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        var repo = new Repository<ProblemDefinition>(_context);
        await repo.AddAsync(new ProblemDefinition
        {
            Id = Guid.NewGuid(), Name = "Problem 1",
            Cities = new List<City>(), CityCount = 0
        });
        await repo.AddAsync(new ProblemDefinition
        {
            Id = Guid.NewGuid(), Name = "Problem 2",
            Cities = new List<City>(), CityCount = 0
        });
        await _context.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_ModifiesEntity()
    {
        var repo = new Repository<ProblemDefinition>(_context);
        var problem = new ProblemDefinition
        {
            Id = Guid.NewGuid(), Name = "Original",
            Cities = new List<City>(), CityCount = 0
        };
        await repo.AddAsync(problem);
        await _context.SaveChangesAsync();

        problem.Name = "Updated";
        repo.Update(problem);
        await _context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(problem.Id);
        retrieved!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_RemovesEntity()
    {
        var repo = new Repository<ProblemDefinition>(_context);
        var problem = new ProblemDefinition
        {
            Id = Guid.NewGuid(), Name = "To Delete",
            Cities = new List<City>(), CityCount = 0
        };
        await repo.AddAsync(problem);
        await _context.SaveChangesAsync();

        repo.Delete(problem);
        await _context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(problem.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var repo = new Repository<ProblemDefinition>(_context);
        var result = await repo.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
