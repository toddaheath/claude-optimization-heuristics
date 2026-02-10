using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Entities;

public class ProblemDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<City> Cities { get; set; } = new();
    public int CityCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
