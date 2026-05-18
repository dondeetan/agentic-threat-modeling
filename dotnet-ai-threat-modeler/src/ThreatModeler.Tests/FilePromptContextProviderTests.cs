using ThreatModeler.Models;
using ThreatModeler.Services;
using Xunit;

namespace ThreatModeler.Tests;

public sealed class FilePromptContextProviderTests
{
    [Fact]
    public async Task GetContextAsync_LoadsRequiredAndRelevantGuidelines()
    {
        var promptRoot = CreatePromptRoot();

        try
        {
            var provider = new FilePromptContextProvider(promptRoot);
            var submission = new Submission
            {
                Components = new() { "Identity API" },
                InternetExposure = "Public",
                SensitiveData = new() { "PII" }
            };

            var context = await provider.GetContextAsync(submission, CancellationToken.None);

            Assert.Equal("system rules", context.SystemRules);
            Assert.Equal("""{ "summary": "string" }""", context.OutputFormat);
            Assert.Contains("threat-modeling-guidelines", context.Guidelines);
            Assert.Contains("stride content", context.Guidelines);
            Assert.Contains("cloud content", context.Guidelines);
            Assert.Contains("identity content", context.Guidelines);
            Assert.Contains("data content", context.Guidelines);
        }
        finally
        {
            Directory.Delete(promptRoot, recursive: true);
        }
    }

    [Fact]
    public async Task GetContextAsync_UsesBaselineGuidelinesForMinimalSubmissions()
    {
        var promptRoot = CreatePromptRoot();

        try
        {
            var provider = new FilePromptContextProvider(promptRoot);
            var context = await provider.GetContextAsync(new Submission(), CancellationToken.None);

            Assert.Contains("threat-modeling-guidelines", context.Guidelines);
            Assert.Contains("stride content", context.Guidelines);
            Assert.DoesNotContain("cloud content", context.Guidelines);
            Assert.DoesNotContain("identity content", context.Guidelines);
            Assert.DoesNotContain("data content", context.Guidelines);
        }
        finally
        {
            Directory.Delete(promptRoot, recursive: true);
        }
    }

    private static string CreatePromptRoot()
    {
        var promptRoot = Path.Combine(Path.GetTempPath(), $"threat-modeler-prompts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(promptRoot);

        File.WriteAllText(Path.Combine(promptRoot, "system-rules.md"), "system rules");
        File.WriteAllText(Path.Combine(promptRoot, "output-format.json"), """{ "summary": "string" }""");
        File.WriteAllText(Path.Combine(promptRoot, "threat-modeling-guidelines.md"), "baseline content");
        File.WriteAllText(Path.Combine(promptRoot, "stride.md"), "stride content");
        File.WriteAllText(Path.Combine(promptRoot, "cloud-controls.md"), "cloud content");
        File.WriteAllText(Path.Combine(promptRoot, "identity-controls.md"), "identity content");
        File.WriteAllText(Path.Combine(promptRoot, "data-protection.md"), "data content");

        return promptRoot;
    }
}
