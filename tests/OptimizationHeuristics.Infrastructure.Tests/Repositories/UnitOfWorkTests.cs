using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OptimizationHeuristics.Core.Entities;
using OptimizationHeuristics.Core.Models;
using OptimizationHeuristics.Infrastructure.Data;
using OptimizationHeuristics.Infrastructure.Repositories;

namespace OptimizationHeuristics.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public void Repository_ReturnsSameInstanceForSameType()
    {
        var repo1 = _unitOfWork.Repository<ProblemDefinition>();
        var repo2 = _unitOfWork.Repository<ProblemDefinition>();
        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Repository_ReturnsDifferentInstancesForDifferentTypes()
    {
        var problemRepo = _unitOfWork.Repository<ProblemDefinition>();
        var configRepo = _unitOfWork.Repository<AlgorithmConfiguration>();
        problemRepo.Should().NotBeSameAs(configRepo);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChanges()
    {
        var repo = _unitOfWork.Repository<ProblemDefinition>();
        await repo.AddAsync(new ProblemDefinition
        {
            Id = Guid.NewGuid(), Name = "Test",
            Cities = new List<City>(), CityCount = 0
        });

        var changes = await _unitOfWork.SaveChangesAsync();
        changes.Should().Be(1);
    }

    public void Dispose() => _unitOfWork.Dispose();
}
