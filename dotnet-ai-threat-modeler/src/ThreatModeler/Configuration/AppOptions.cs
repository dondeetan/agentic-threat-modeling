namespace ThreatModeler.Configuration;

public sealed class AppOptions
{
    public string AnalyzerType { get; set; } = AnalyzerTypes.OpenApi;
    public bool UseInMemoryStore { get; set; } = true;
}

public static class AnalyzerTypes
{
    public const string OpenApi = "openapi";
    public const string OpenAi = "openai";
    public const string AzureOpenAi = "azure-openai";
    public const string Mock = "mock";
}

public sealed class CosmosOptions
{
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
    public string Database { get; set; } = "ThreatModeler";
    public string SubmissionsContainer { get; set; } = "submissions";
    public string RunsContainer { get; set; } = "threatModelRuns";
}
