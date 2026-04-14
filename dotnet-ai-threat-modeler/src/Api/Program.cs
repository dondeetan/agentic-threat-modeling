using ThreatModeler.Configuration;
using ThreatModeler.Models;
using ThreatModeler.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/submit", async (SubmissionRequest request, ISubmissionStore store, CancellationToken ct) =>
{
    var submission = new Submission
    {
        TenantId = request.TenantId,
        ApplicationName = request.ApplicationName,
        BusinessPurpose = request.BusinessPurpose,
        ArchitectureSummary = request.ArchitectureSummary,
        Components = request.Components,
        DataFlows = request.DataFlows,
        TrustBoundaries = request.TrustBoundaries,
        AuthenticationDetails = request.AuthenticationDetails,
        SensitiveData = request.SensitiveData,
        InternetExposure = request.InternetExposure,
        ExistingControls = request.ExistingControls,
        Assumptions = request.Assumptions
    };

    await store.CreateSubmissionAsync(submission, ct);
    return Results.Ok(new { id = submission.Id, tenantId = submission.TenantId, status = submission.Status });
});

app.MapPost("/analyze/{submissionId}", async (string submissionId, string tenantId, ISubmissionStore store, IAnalyzer analyzer, CancellationToken ct) =>
{
    var submission = await store.GetSubmissionAsync(tenantId, submissionId, ct);
    if (submission is null)
    {
        return Results.NotFound(new { message = "Submission not found." });
    }

    var result = await analyzer.AnalyzeAsync(submission, ct);
    var run = new ThreatModelRun
    {
        TenantId = tenantId,
        SubmissionId = submissionId,
        AnalyzerType = analyzer.AnalyzerType,
        Result = result
    };

    await store.CreateRunAsync(run, ct);
    return Results.Ok(new { runId = run.Id, submissionId, status = run.Status });
});

app.MapGet("/results/{runId}", async (string runId, string tenantId, ISubmissionStore store, CancellationToken ct) =>
{
    var run = await store.GetRunAsync(tenantId, runId, ct);
    return run is null
        ? Results.NotFound(new { message = "Run not found." })
        : Results.Ok(run);
});

app.Run();
