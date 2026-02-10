using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Integration.Tests;

public class ProblemDefinitionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProblemDefinitionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAndGetProblemDefinition_WorksEndToEnd()
    {
        var request = new CreateProblemDefinitionRequest(
            "Integration Test Problem", "Test description",
            new List<CityDto>
            {
                new(0, 0.0, 0.0),
                new(1, 10.0, 0.0),
                new(2, 10.0, 10.0),
                new(3, 0.0, 10.0)
            });

        var createResponse = await _client.PostAsJsonAsync("/api/v1/problem-definitions", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProblemDefinitionResponse>>();
        created!.Success.Should().BeTrue();
        created.Data!.Name.Should().Be("Integration Test Problem");
        created.Data.CityCount.Should().Be(4);

        var getResponse = await _client.GetAsync($"/api/v1/problem-definitions/{created.Data.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/problem-definitions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/problem-definitions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
