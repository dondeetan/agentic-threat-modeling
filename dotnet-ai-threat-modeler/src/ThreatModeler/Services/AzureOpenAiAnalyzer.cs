using System.Text.Json;
using ThreatModeler.Configuration;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class AzureOpenAiAnalyzer : IAnalyzer
{
    private readonly AzureOpenAiOptions _options;
    public string AnalyzerType => "azure-openai";

    public AzureOpenAiAnalyzer(AzureOpenAiOptions options)
    {
        _options = options;
    }

    public Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI configuration is missing.");
        }

        // This is intentionally a safe scaffold. Replace with SDK invocation when you wire your Azure deployment.
        var fallback = new
        {
            summary = "Azure OpenAI adapter is configured as a scaffold. Wire the SDK call here for live analysis.",
            assets = submission.Components,
            trustBoundaries = submission.TrustBoundaries,
            threats = Array.Empty<object>(),
            topPriorities = new[] { "Implement live Azure OpenAI call in AzureOpenAiAnalyzer." }
        };

        return Task.FromResult<object>(fallback);
    }
}
