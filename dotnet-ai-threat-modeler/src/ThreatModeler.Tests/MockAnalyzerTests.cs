using System.Text.Json;
using ThreatModeler.Models;
using ThreatModeler.Services;
using Xunit;

namespace ThreatModeler.Tests;

public sealed class MockAnalyzerTests
{
    [Fact]
    public async Task AnalyzeAsync_ReturnsExpectedThreatStructure()
    {
        var analyzer = new MockAnalyzer();
        var submission = new Submission
        {
            ApplicationName = "Payments API",
            Components = new() { "Gateway", "Processor" },
            TrustBoundaries = new() { "Internet", "App to Data" }
        };

        var result = await analyzer.AnalyzeAsync(submission, CancellationToken.None);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.Equal(
            "Payments API should prioritize identity, trust boundary, and data protection controls.",
            json.RootElement.GetProperty("summary").GetString());
        Assert.Equal(2, json.RootElement.GetProperty("assets").GetArrayLength());
        Assert.Equal(3, json.RootElement.GetProperty("threats").GetArrayLength());
        Assert.Equal("Gateway", json.RootElement.GetProperty("threats")[0].GetProperty("component").GetString());
        Assert.Equal("Processor", json.RootElement.GetProperty("threats")[1].GetProperty("component").GetString());
    }
}
