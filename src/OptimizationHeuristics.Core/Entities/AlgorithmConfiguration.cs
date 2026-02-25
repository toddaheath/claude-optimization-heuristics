using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Entities;

public class AlgorithmConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AlgorithmType AlgorithmType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public int MaxIterations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
