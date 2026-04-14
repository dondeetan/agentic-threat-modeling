using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class MockAnalyzer : IAnalyzer
{
    public string AnalyzerType => "mock";

    public Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        var components = submission.Components;
        var result = new
        {
            summary = $"{submission.ApplicationName} should prioritize identity, trust boundary, and data protection controls.",
            assets = components.Select(c => new { name = c, type = "component", sensitivity = "medium" }).ToList(),
            trustBoundaries = submission.TrustBoundaries,
            threats = new[]
            {
                new
                {
                    id = "TM-001",
                    strideCategory = "Spoofing",
                    component = components.FirstOrDefault() ?? "Unknown",
                    threatStatement = "Identity tokens could be spoofed or replayed if validation is weak.",
                    mitigations = new[] { "Entra ID", "Validate issuer/audience", "Short token lifetimes" }
                },
                new
                {
                    id = "TM-002",
                    strideCategory = "Tampering",
                    component = components.LastOrDefault() ?? "Unknown",
                    threatStatement = "Data could be modified through over-privileged access or weak service trust.",
                    mitigations = new[] { "Managed Identity", "Least privilege RBAC", "Private Endpoints" }
                },
                new
                {
                    id = "TM-003",
                    strideCategory = "Information Disclosure",
                    component = components.LastOrDefault() ?? "Unknown",
                    threatStatement = "Sensitive data may leak through logs, queries, or storage misconfiguration.",
                    mitigations = new[] { "Encryption", "Log scrubbing", "Data classification" }
                }
            },
            topPriorities = new[]
            {
                "Enforce managed identity and least privilege.",
                "Use private connectivity for data services.",
                "Protect logs from sensitive data leakage."
            }
        };

        return Task.FromResult<object>(result);
    }
}
