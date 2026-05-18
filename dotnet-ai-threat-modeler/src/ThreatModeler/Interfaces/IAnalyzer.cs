using ThreatModeler.Models;

namespace ThreatModeler.Interfaces;

/// <summary>
/// Analyzes a submitted system design and returns a threat model result.
/// </summary>
public interface IAnalyzer
{
    /// <summary>
    /// Gets the configuration key used to select this analyzer implementation.
    /// </summary>
    string AnalyzerType { get; }

    /// <summary>
    /// Runs threat modeling analysis for the supplied submission.
    /// </summary>
    /// <param name="submission">The submitted system design to analyze.</param>
    /// <param name="cancellationToken">A token used to cancel the analysis request.</param>
    /// <returns>The analyzer-specific threat model result.</returns>
    Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default);
}
