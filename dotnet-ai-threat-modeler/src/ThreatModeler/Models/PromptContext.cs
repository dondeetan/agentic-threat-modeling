namespace ThreatModeler.Models;

// Value object: retrieved prompt fragments move through the analyzer as one immutable unit.
public sealed record PromptContext(
    string SystemRules,
    string Guidelines,
    string OutputFormat);
