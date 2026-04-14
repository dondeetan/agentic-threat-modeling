namespace ThreatModeler.Configuration;

public sealed class AppOptions
{
    public bool UseMockAnalyzer { get; set; } = true;
    public bool UseInMemoryStore { get; set; } = true;
}

public sealed class CosmosOptions
{
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
    public string Database { get; set; } = "ThreatModeler";
    public string SubmissionsContainer { get; set; } = "submissions";
    public string RunsContainer { get; set; } = "threatModelRuns";
}

public sealed class AzureOpenAiOptions
{
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string ApiVersion { get; set; } = "2024-10-21";
    public string Deployment { get; set; } = "gpt-4o-mini";
}
