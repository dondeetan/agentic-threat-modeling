using System.Text.Json;
using ThreatModeler.Interfaces;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class PromptBuilder : IPromptBuilder
{
    public string Build(Submission submission, PromptContext context)
    {
        // Single Responsibility Principle: this class only composes the final model prompt.
        var payload = JsonSerializer.Serialize(submission, new JsonSerializerOptions { WriteIndented = true });

        return
            $"""
            # System Rules
            {context.SystemRules}

            # Retrieved Guidelines
            {context.Guidelines}

            # Output Format
            {context.OutputFormat}

            # Submission
            {payload}
            """;
    }
}
