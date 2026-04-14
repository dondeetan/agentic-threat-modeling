using ThreatModeler.Configuration;
using ThreatModeler.Models;
using ThreatModeler.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<AzureOpenAiOptions>(builder.Configuration.GetSection("AzureOpenAI"));

var appOptions = builder.Configuration.GetSection("App").Get<AppOptions>() ?? new AppOptions();
var cosmosOptions = builder.Configuration.GetSection("Cosmos").Get<CosmosOptions>() ?? new CosmosOptions();
var openAiOptions = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAiOptions>() ?? new AzureOpenAiOptions();

builder.Services.AddSingleton<ISubmissionStore>(_ =>
    appOptions.UseInMemoryStore ? new InMemorySubmissionStore() : new CosmosSubmissionStore(cosmosOptions));

builder.Services.AddSingleton<IAnalyzer>(_ =>
    appOptions.UseMockAnalyzer ? new MockAnalyzer() : new AzureOpenAiAnalyzer(openAiOptions));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/", () => Results.Ok(new
{
    service = "AI Threat Modeler API",
    endpoints = new[]
    {
        "GET /health",
        "POST /submit",
        "POST /analyze/{submissionId}?tenantId={tenantId}",
        "GET /results/{runId}?tenantId={tenantId}"
    }
}));

app.MapPost("/submit", async (SubmissionRequest request, ISubmissionStore store, CancellationToken ct) =>
{
    var response = await SubmissionWorkflow.CreateSubmissionAsync(request, store, ct);
    return Results.Ok(response);
});

app.MapPost("/analyze/{submissionId}", async (string submissionId, string tenantId, ISubmissionStore store, IAnalyzer analyzer, CancellationToken ct) =>
{
    var response = await SubmissionWorkflow.AnalyzeSubmissionAsync(submissionId, tenantId, store, analyzer, ct);
    return response is null
        ? Results.NotFound(new { message = "Submission not found." })
        : Results.Ok(response);
});

app.MapGet("/results/{runId}", async (string runId, string tenantId, ISubmissionStore store, CancellationToken ct) =>
{
    var response = await SubmissionWorkflow.GetResultsAsync(runId, tenantId, store, ct);
    return response is null
        ? Results.NotFound(new { message = "Run not found." })
        : Results.Ok(response);
});

app.Run();

public partial class Program;
