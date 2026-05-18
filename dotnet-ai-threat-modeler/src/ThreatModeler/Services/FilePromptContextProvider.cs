using System.Text;
using ThreatModeler.Interfaces;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class FilePromptContextProvider : IPromptContextProvider
{
    private const string SystemRulesFileName = "system-rules.md";
    private const string OutputFormatFileName = "output-format.json";
    private readonly string _promptRootDirectory;

    public FilePromptContextProvider()
        : this(Path.Combine(AppContext.BaseDirectory, "Prompts"))
    {
    }

    public FilePromptContextProvider(string promptRootDirectory)
    {
        // Dependency Inversion Principle: callers depend on IPromptContextProvider, not this file-system detail.
        _promptRootDirectory = promptRootDirectory;
    }

    public async Task<PromptContext> GetContextAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        // Template-style flow: required prompt fragments are always loaded, optional guidance is selected by submission context.
        var systemRules = await ReadRequiredPromptFileAsync(SystemRulesFileName, cancellationToken);
        var outputFormat = await ReadRequiredPromptFileAsync(OutputFormatFileName, cancellationToken);
        var guidelineFileNames = SelectGuidelineFileNames(submission);

        var guidelines = new StringBuilder();
        foreach (var guidelineFileName in guidelineFileNames)
        {
            var content = await ReadRequiredPromptFileAsync(guidelineFileName, cancellationToken);
            guidelines.AppendLine($"## {Path.GetFileNameWithoutExtension(guidelineFileName)}");
            guidelines.AppendLine(content.Trim());
            guidelines.AppendLine();
        }

        return new PromptContext(systemRules.Trim(), guidelines.ToString().Trim(), outputFormat.Trim());
    }

    private static IReadOnlyList<string> SelectGuidelineFileNames(Submission submission)
    {
        // Open/Closed Principle: new guideline files can be added here without changing analyzer transport logic.
        var guidelineFileNames = new List<string>
        {
            "threat-modeling-guidelines.md",
            "stride.md"
        };

        if (!string.IsNullOrWhiteSpace(submission.InternetExposure) ||
            submission.TrustBoundaries.Count > 0 ||
            submission.Components.Count > 0)
        {
            guidelineFileNames.Add("cloud-controls.md");
        }

        if (!string.IsNullOrWhiteSpace(submission.AuthenticationDetails) ||
            HasAnyValueContaining(submission.Components, "identity", "auth", "login", "token"))
        {
            guidelineFileNames.Add("identity-controls.md");
        }

        if (submission.SensitiveData.Count > 0 ||
            HasAnyValueContaining(submission.DataFlows, "pii", "payment", "secret", "credential"))
        {
            guidelineFileNames.Add("data-protection.md");
        }

        return guidelineFileNames;
    }

    private async Task<string> ReadRequiredPromptFileAsync(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_promptRootDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Prompt context file '{fileName}' was not found.", path);
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    private static bool HasAnyValueContaining(IEnumerable<string> values, params string[] needles)
    {
        return values.Any(value =>
            needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase)));
    }
}
