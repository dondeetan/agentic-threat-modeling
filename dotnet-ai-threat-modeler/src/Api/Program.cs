using ChatClientShared.AzureOpenAIChatClientShared;
using ChatClientShared.OpenAIChatClientShared;
using ThreatModeler.Configuration;
using ThreatModeler.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8085");

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));

var appOptions = builder.Configuration.GetSection("App").Get<AppOptions>() ?? new AppOptions();
var cosmosOptions = builder.Configuration.GetSection("Cosmos").Get<CosmosOptions>() ?? new CosmosOptions();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Factory Method pattern: configuration chooses the repository implementation while callers depend on ISubmissionStore.
builder.Services.AddSingleton<ISubmissionStore>(_ =>
    appOptions.UseInMemoryStore ? new InMemorySubmissionStore() : new CosmosSubmissionStore(cosmosOptions));

// Dependency Inversion Principle: the API depends on workflow/analyzer abstractions, not concrete implementations.
builder.Services.AddSingleton<IAnalyzer, MockAnalyzer>();
builder.Services.AddSingleton<IAnalyzer>(_ =>
    new ChatClientAnalyzer(
        AnalyzerTypes.OpenAi,
        () => OpenAIChatClientFactory.Create(builder.Configuration)));
builder.Services.AddSingleton<IAnalyzer>(_ =>
    new ChatClientAnalyzer(
        AnalyzerTypes.AzureOpenAi,
        () => AzureOpenAIChatClientFactory.Create(builder.Configuration)));
builder.Services.AddSingleton<IAnalyzerFactory, AnalyzerFactory>();
builder.Services.AddSingleton<ISubmissionWorkflow>(serviceProvider =>
    new SubmissionWorkflow(
        serviceProvider.GetRequiredService<ISubmissionStore>(),
        serviceProvider.GetRequiredService<IAnalyzerFactory>(),
        appOptions.AnalyzerType));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Front Controller pattern: ASP.NET Core routing dispatches requests to controller actions.
app.MapControllers();

app.Run();

public partial class Program;
