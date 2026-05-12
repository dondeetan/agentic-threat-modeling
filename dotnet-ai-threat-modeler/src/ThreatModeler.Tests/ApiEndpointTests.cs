using ThreatModeler.Models;
using ThreatModeler.Services;
using Xunit;

namespace ThreatModeler.Tests;

public sealed class ApiEndpointTests
{
    [Fact]
    public async Task CreateSubmissionAsync_PersistsSubmissionAndReturnsOk()
    {
        var store = new InMemorySubmissionStore();
        var request = CreateRequest();
        var workflow = CreateWorkflow(store, new MockAnalyzer());

        var response = await workflow.CreateSubmissionAsync(request, CancellationToken.None);

        Assert.Equal(request.TenantId, response.TenantId);
        Assert.Equal("submitted", response.Status);

        Assert.False(string.IsNullOrWhiteSpace(response.Id));

        var stored = await store.GetSubmissionAsync(request.TenantId, response.Id, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal(request.ApplicationName, stored!.ApplicationName);
        Assert.Equal(request.Components, stored.Components);
    }

    [Fact]
    public async Task AnalyzeSubmissionAsync_ReturnsNotFoundWhenSubmissionDoesNotExist()
    {
        var store = new InMemorySubmissionStore();
        var workflow = CreateWorkflow(store, new MockAnalyzer());

        var result = await workflow.AnalyzeSubmissionAsync(
            "missing-submission",
            "tenant-a",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeSubmissionAsync_CreatesRunAndReturnsOk()
    {
        var store = new InMemorySubmissionStore();
        var analyzer = new MockAnalyzer();
        var workflow = CreateWorkflow(store, analyzer);
        var submission = new Submission
        {
            TenantId = "tenant-a",
            ApplicationName = "Claims API",
            Components = new() { "React SPA", "API" },
            TrustBoundaries = new() { "Internet to App" }
        };

        await store.CreateSubmissionAsync(submission, CancellationToken.None);

        var response = await workflow.AnalyzeSubmissionAsync(
            submission.Id,
            submission.TenantId,
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(submission.Id, response!.SubmissionId);
        Assert.Equal("completed", response.Status);
        Assert.False(string.IsNullOrWhiteSpace(response.RunId));

        var storedRun = await store.GetRunAsync(submission.TenantId, response.RunId, CancellationToken.None);
        Assert.NotNull(storedRun);
        Assert.Equal("mock", storedRun!.AnalyzerType);
        Assert.Equal(submission.Id, storedRun.SubmissionId);
    }

    [Fact]
    public async Task GetResultsAsync_ReturnsStoredRun()
    {
        var store = new InMemorySubmissionStore();
        var workflow = CreateWorkflow(store, new MockAnalyzer());
        var run = new ThreatModelRun
        {
            TenantId = "tenant-a",
            SubmissionId = "sub-123",
            AnalyzerType = "mock",
            Result = new { summary = "ok" }
        };

        await store.CreateRunAsync(run, CancellationToken.None);

        var response = await workflow.GetResultsAsync(
            run.Id,
            run.TenantId,
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(run.Id, response!.Id);
        Assert.Equal(run.SubmissionId, response.SubmissionId);
        Assert.Equal(run.AnalyzerType, response.AnalyzerType);
    }

    [Fact]
    public async Task GetResultsAsync_ReturnsNotFoundForMissingRun()
    {
        var workflow = CreateWorkflow(new InMemorySubmissionStore(), new MockAnalyzer());

        var result = await workflow.GetResultsAsync(
            "missing-run",
            "tenant-a",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeSubmissionAsync_UsesOpenApiAnalyzerByDefault()
    {
        var store = new InMemorySubmissionStore();
        var workflow = CreateWorkflow(store, new OpenApiAnalyzer(), "openapi");
        var submission = new Submission
        {
            TenantId = "tenant-a",
            ApplicationName = "Claims API",
            Components = new() { "Claims API" },
            OpenApiDocument = """
            {
              "openapi": "3.0.1",
              "paths": {
                "/claims": {
                  "get": {},
                  "post": {}
                }
              }
            }
            """
        };

        await store.CreateSubmissionAsync(submission, CancellationToken.None);

        var response = await workflow.AnalyzeSubmissionAsync(
            submission.Id,
            submission.TenantId,
            CancellationToken.None);

        Assert.NotNull(response);
        var storedRun = await store.GetRunAsync(submission.TenantId, response!.RunId, CancellationToken.None);
        Assert.NotNull(storedRun);
        Assert.Equal("openapi", storedRun!.AnalyzerType);
    }

    private static SubmissionRequest CreateRequest()
    {
        return new SubmissionRequest
        {
            TenantId = "tenant-a",
            ApplicationName = "Claims API",
            BusinessPurpose = "Process claims",
            ArchitectureSummary = "SPA to API to data store",
            Components = new() { "React SPA", "API", "Storage" },
            DataFlows = new() { "SPA to API", "API to Storage" },
            TrustBoundaries = new() { "Internet", "App to Data" },
            OpenApiDocument = """
            {
              "openapi": "3.0.1",
              "paths": {
                "/claims": {
                  "get": {},
                  "post": {}
                }
              }
            }
            """,
            AuthenticationDetails = "Entra ID",
            SensitiveData = new() { "PII" },
            InternetExposure = "Public API",
            ExistingControls = new() { "WAF" },
            Assumptions = new() { "No public DB access" }
        };
    }

    private static SubmissionWorkflow CreateWorkflow(
        ISubmissionStore store,
        IAnalyzer analyzer,
        string analyzerType = "mock")
    {
        var factory = new AnalyzerFactory(new[] { analyzer });
        return new SubmissionWorkflow(store, factory, analyzerType);
    }
}
