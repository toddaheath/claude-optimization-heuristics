using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Integration.Tests;

public class ProblemDefinitionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProblemDefinitionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(Guid.NewGuid());
    }

    private static CreateProblemDefinitionRequest MakeRequest(string name = "Test Problem") =>
        new(name, "Test description",
            new List<CityDto>
            {
                new(0, 0.0, 0.0),
                new(1, 10.0, 0.0),
                new(2, 10.0, 10.0),
                new(3, 0.0, 10.0)
            });

    [Fact]
    public async Task CreateAndGetProblemDefinition_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/problem-definitions", MakeRequest("Integration Test Problem"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProblemDefinitionResponse>>();
        created!.Success.Should().BeTrue();
        created.Data!.Name.Should().Be("Integration Test Problem");
        created.Data.CityCount.Should().Be(4);

        var getResponse = await _client.GetAsync($"/api/v1/problem-definitions/{created.Data.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedResponse()
    {
        await _client.PostAsJsonAsync("/api/v1/problem-definitions", MakeRequest("Problem A"));
        await _client.PostAsJsonAsync("/api/v1/problem-definitions", MakeRequest("Problem B"));

        var response = await _client.GetAsync("/api/v1/problem-definitions?page=1&pageSize=50");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Delete_RemovesProblem()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/problem-definitions", MakeRequest("To Delete"));
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProblemDefinitionResponse>>();
        var id = created!.Data!.Id;

        var deleteResponse = await _client.DeleteAsync($"/api/v1/problem-definitions/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/problem-definitions/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/v1/problem-definitions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/problem-definitions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var request = new CreateProblemDefinitionRequest(
            "", null, new List<CityDto> { new(0, 0.0, 0.0), new(1, 1.0, 1.0) });
        var response = await _client.PostAsJsonAsync("/api/v1/problem-definitions", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_SingleCity_ReturnsBadRequest()
    {
        var request = new CreateProblemDefinitionRequest(
            "One City", null, new List<CityDto> { new(0, 0.0, 0.0) });
        var response = await _client.PostAsJsonAsync("/api/v1/problem-definitions", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unauthenticated_Returns401()
    {
        using var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/v1/problem-definitions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
