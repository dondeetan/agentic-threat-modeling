namespace ThreatModeler.Interfaces;

/// <summary>
/// Resolves analyzer implementations by their configured analyzer type.
/// </summary>
public interface IAnalyzerFactory
{
    /// <summary>
    /// Creates or retrieves the analyzer registered for the requested type.
    /// </summary>
    /// <param name="analyzerType">The analyzer type requested by configuration or callers.</param>
    /// <returns>The analyzer that handles the requested type.</returns>
    IAnalyzer Create(string analyzerType);
}
