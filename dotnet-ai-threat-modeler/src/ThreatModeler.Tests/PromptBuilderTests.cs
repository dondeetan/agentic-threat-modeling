using ThreatModeler.Models;
using ThreatModeler.Services;
using Xunit;

namespace ThreatModeler.Tests;

public sealed class PromptBuilderTests
{
    [Fact]
    public void Build_ComposesRetrievedContextAndSubmissionPayload()
    {
        var builder = new PromptBuilder();
        var submission = new Submission
        {
            ApplicationName = "Payments API",
            BusinessPurpose = "Process card payments",
            Components = new() { "Gateway" }
        };
        var context = new PromptContext(
            "System rule text",
            "Guideline text",
            """{ "summary": "string" }""");

        var prompt = builder.Build(submission, context);

        Assert.Contains("# System Rules", prompt);
        Assert.Contains("System rule text", prompt);
        Assert.Contains("# Retrieved Guidelines", prompt);
        Assert.Contains("Guideline text", prompt);
        Assert.Contains("# Output Format", prompt);
        Assert.Contains("""{ "summary": "string" }""", prompt);
        Assert.Contains("\"ApplicationName\": \"Payments API\"", prompt);
    }
}
