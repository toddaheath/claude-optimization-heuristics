using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Entities;

public class OptimizationRun
{
    public Guid Id { get; set; }
    public Guid AlgorithmConfigurationId { get; set; }
    public Guid ProblemDefinitionId { get; set; }
    public RunStatus Status { get; set; }
    public double? BestDistance { get; set; }
    public List<int>? BestRoute { get; set; }
    public List<IterationResult>? IterationHistory { get; set; }
    public int TotalIterations { get; set; }
    public long ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid? UserId { get; set; }
    public AlgorithmConfiguration? AlgorithmConfiguration { get; set; }
    public ProblemDefinition? ProblemDefinition { get; set; }
    public User? User { get; set; }
}
