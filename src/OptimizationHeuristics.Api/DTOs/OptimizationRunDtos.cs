using OptimizationHeuristics.Core.Enums;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Api.DTOs;

public record RunOptimizationRequest(
    Guid AlgorithmConfigurationId,
    Guid ProblemDefinitionId
);

public record OptimizationRunResponse(
    Guid Id,
    Guid AlgorithmConfigurationId,
    Guid ProblemDefinitionId,
    RunStatus Status,
    double? BestDistance,
    List<int>? BestRoute,
    List<IterationResult>? IterationHistory,
    int TotalIterations,
    long ExecutionTimeMs,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record RunProgressResponse(
    Guid RunId,
    RunStatus Status,
    List<IterationResult> IterationHistory,
    double? BestDistance,
    long ExecutionTimeMs,
    string? ErrorMessage
);
