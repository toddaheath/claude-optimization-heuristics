using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Api.DTOs;

public record CreateAlgorithmConfigurationRequest(
    string Name,
    string? Description,
    AlgorithmType AlgorithmType,
    Dictionary<string, object> Parameters,
    int MaxIterations
);

public record UpdateAlgorithmConfigurationRequest(
    string Name,
    string? Description,
    AlgorithmType AlgorithmType,
    Dictionary<string, object> Parameters,
    int MaxIterations
);

public record AlgorithmConfigurationResponse(
    Guid Id,
    string Name,
    string? Description,
    AlgorithmType AlgorithmType,
    Dictionary<string, object> Parameters,
    int MaxIterations,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
