namespace ThreatModeler.Configuration;

public sealed class AppOptions
{
    // Options pattern: strongly typed settings keep configuration access out of business services.
    public string AnalyzerType { get; set; } = AnalyzerTypes.OpenAi;
    public bool UseInMemoryStore { get; set; } = true;
}

public static class AnalyzerTypes
{
    // Replace Magic String: central constants keep configured strategy names consistent across the solution.
    public const string OpenAi = "openai";
    public const string AzureOpenAi = "azure-openai";
    public const string Mock = "mock";
}

public sealed class CosmosOptions
{
    // Options pattern: Cosmos settings are grouped as one configuration object.
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
    public string Database { get; set; } = "ThreatModeler";
    public string SubmissionsContainer { get; set; } = "submissions";
    public string RunsContainer { get; set; } = "threatModelRuns";
}
