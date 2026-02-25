using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptimizationHeuristics.Infrastructure.Data;

/// <summary>
/// Used by EF Core design-time tools (dotnet ef migrations add) to create
/// ApplicationDbContext without executing the full startup pipeline.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=optimization_heuristics;Username=postgres;Password=postgres");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
