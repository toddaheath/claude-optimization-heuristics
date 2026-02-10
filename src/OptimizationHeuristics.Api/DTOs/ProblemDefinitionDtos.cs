using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Api.DTOs;

public record CreateProblemDefinitionRequest(
    string Name,
    string? Description,
    List<CityDto> Cities
);

public record CityDto(int Id, double X, double Y, string? Name = null)
{
    public City ToModel() => new(Id, X, Y, Name);
    public static CityDto FromModel(City city) => new(city.Id, city.X, city.Y, city.Name);
}

public record ProblemDefinitionResponse(
    Guid Id,
    string Name,
    string? Description,
    List<CityDto> Cities,
    int CityCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
