using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Integration.Tests;

public class OptimizationRunIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OptimizationRunIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient(Guid.NewGuid());
    }

    [Fact]
    public async Task CreateConfig_CreateProblem_Run_PollProgress_Completes()
    {
        // Create a problem
        var problemRequest = new CreateProblemDefinitionRequest(
            "Run Test Problem", "Integration test",
            new List<CityDto>
            {
                new(0, 0.0, 0.0),
                new(1, 10.0, 0.0),
                new(2, 10.0, 10.0),
                new(3, 0.0, 10.0)
            });
        var problemResponse = await _client.PostAsJsonAsync("/api/v1/problem-definitions", problemRequest);
        problemResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var problem = await problemResponse.Content.ReadFromJsonAsync<ApiResponse<ProblemDefinitionResponse>>();
        var problemId = problem!.Data!.Id;

        // Create an algorithm configuration (small iterations for speed)
        // Use JsonDocument to extract ID, avoiding Dictionary<string, object> deserialization issues
        var configRequest = new CreateAlgorithmConfigurationRequest(
            "SA Test", null, AlgorithmType.SimulatedAnnealing,
            new Dictionary<string, object>
            {
                { "initialTemperature", 1000.0 },
                { "coolingRate", 0.95 },
                { "minTemperature", 0.1 }
            },
            20);
        var configResponse = await _client.PostAsJsonAsync("/api/v1/algorithm-configurations", configRequest);
        configResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var configJson = await JsonDocument.ParseAsync(await configResponse.Content.ReadAsStreamAsync());
        var configId = configJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Start a run — also use JsonDocument since response contains iteration history with complex types
        var runRequest = new RunOptimizationRequest(configId, problemId);
        var runResponse = await _client.PostAsJsonAsync("/api/v1/optimization-runs", runRequest);
        runResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var runJson = await JsonDocument.ParseAsync(await runResponse.Content.ReadAsStreamAsync());
        var runData = runJson.RootElement.GetProperty("data");
        var runId = runData.GetProperty("id").GetGuid();
        runData.GetProperty("status").GetString().Should().Be(nameof(RunStatus.Running));

        // Poll until the run finishes. With only 20 iterations the run may complete
        // before the first poll, in which case the progress store is already cleaned up
        // and the endpoint returns 404. Fall back to checking the DB record directly.
        var deadline = DateTime.UtcNow.AddSeconds(10);
        string? finalStatus = null;
        while (DateTime.UtcNow < deadline)
        {
            var progressResponse = await _client.GetAsync($"/api/v1/optimization-runs/{runId}/progress");
            if (progressResponse.StatusCode == HttpStatusCode.OK)
            {
                var progressJson = await JsonDocument.ParseAsync(await progressResponse.Content.ReadAsStreamAsync());
                var statusStr = progressJson.RootElement.GetProperty("data").GetProperty("status").GetString();
                if (statusStr != nameof(RunStatus.Running))
                {
                    finalStatus = statusStr;
                    break;
                }
            }
            else
            {
                // Progress store cleaned up — check DB record instead
                var dbResponse = await _client.GetAsync($"/api/v1/optimization-runs/{runId}");
                if (dbResponse.StatusCode == HttpStatusCode.OK)
                {
                    var dbJson = await JsonDocument.ParseAsync(await dbResponse.Content.ReadAsStreamAsync());
                    var dbStatus = dbJson.RootElement.GetProperty("data").GetProperty("status").GetString();
                    if (dbStatus != nameof(RunStatus.Running))
                    {
                        finalStatus = dbStatus;
                        break;
                    }
                }
            }
            await Task.Delay(200);
        }

        finalStatus.Should().Be(nameof(RunStatus.Completed));

        // Fetch the final run from DB
        var finalRunResponse = await _client.GetAsync($"/api/v1/optimization-runs/{runId}");
        finalRunResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalRunJson = await JsonDocument.ParseAsync(await finalRunResponse.Content.ReadAsStreamAsync());
        var finalRunData = finalRunJson.RootElement.GetProperty("data");
        finalRunData.GetProperty("status").GetString().Should().Be(nameof(RunStatus.Completed));
        finalRunData.GetProperty("bestDistance").GetDouble().Should().BeGreaterThan(0);
        finalRunData.GetProperty("totalIterations").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/optimization-runs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/optimization-runs/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
