using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Integration.Tests;

public class AlgorithmConfigurationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AlgorithmConfigurationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(Guid.NewGuid());
    }

    private static CreateAlgorithmConfigurationRequest MakeRequest(string name = "SA Config") =>
        new(name, "Test config", AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>
            {
                { "initialTemperature", 1000.0 },
                { "coolingRate", 0.99 }
            }, 100);

    private static JsonElement GetData(JsonDocument doc) => doc.RootElement.GetProperty("data");

    [Fact]
    public async Task CreateAndGet_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", MakeRequest());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var doc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = GetData(doc);
        data.GetProperty("name").GetString().Should().Be("SA Config");
        data.GetProperty("algorithmType").GetString().Should().Be(nameof(AlgorithmType.SimulatedAnnealing));
        data.GetProperty("maxIterations").GetInt32().Should().Be(100);
        var id = data.GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync($"/api/v1/algorithm-configurations/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDoc = await JsonDocument.ParseAsync(await getResponse.Content.ReadAsStreamAsync());
        GetData(getDoc).GetProperty("id").GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedResponse()
    {
        await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", MakeRequest("Config A"));
        await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", MakeRequest("Config B"));

        var response = await _client.GetAsync("/api/v1/algorithm-configurations?page=1&pageSize=50");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = GetData(doc);
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", MakeRequest("Before Update"));
        var createDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var id = GetData(createDoc).GetProperty("id").GetGuid();

        var updateRequest = new UpdateAlgorithmConfigurationRequest(
            "After Update", "Updated description", AlgorithmType.GeneticAlgorithm,
            new Dictionary<string, object> { { "populationSize", 50.0 } }, 200);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/algorithm-configurations/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateDoc = await JsonDocument.ParseAsync(await updateResponse.Content.ReadAsStreamAsync());
        var data = GetData(updateDoc);
        data.GetProperty("name").GetString().Should().Be("After Update");
        data.GetProperty("algorithmType").GetString().Should().Be(nameof(AlgorithmType.GeneticAlgorithm));
        data.GetProperty("maxIterations").GetInt32().Should().Be(200);
    }

    [Fact]
    public async Task Delete_RemovesConfig()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", MakeRequest("To Delete"));
        var createDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var id = GetData(createDoc).GetProperty("id").GetGuid();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/algorithm-configurations/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/algorithm-configurations/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/algorithm-configurations/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_InvalidName_ReturnsBadRequest()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>(), 100);

        var response = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unauthenticated_Returns401()
    {
        using var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/v1/algorithm-configurations");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_InvalidParameter_ReturnsBadRequest()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "Bad Params", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object> { { "coolingRate", 5.0 } }, 100);

        var response = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_UnknownParameter_ReturnsBadRequest()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "Unknown Param", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object> { { "bogusParam", 42.0 } }, 100);

        var response = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_GA_CrossParameterViolation_ReturnsBadRequest()
    {
        var request = new CreateAlgorithmConfigurationRequest(
            "Bad GA", null, AlgorithmType.GeneticAlgorithm,
            new Dictionary<string, object>
            {
                { "populationSize", 10.0 },
                { "tournamentSize", 20.0 },
                { "mutationRate", 0.02 },
                { "eliteCount", 2.0 }
            }, 100);

        var response = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
