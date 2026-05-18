using ThreatModeler.Configuration;
using ThreatModeler.Interfaces;

namespace ThreatModeler.Services;

public sealed class AnalyzerFactory(IEnumerable<IAnalyzer> analyzers) : IAnalyzerFactory
{
    private readonly IReadOnlyDictionary<string, IAnalyzer> _analyzers =
        analyzers.ToDictionary(analyzer => analyzer.AnalyzerType, StringComparer.OrdinalIgnoreCase);

    public IAnalyzer Create(string analyzerType)
    {
        // Factory Method: callers ask for an analyzer by capability while construction stays in one place.
        var requestedType = string.IsNullOrWhiteSpace(analyzerType)
            ? AnalyzerTypes.OpenAi
            : analyzerType;

        if (_analyzers.TryGetValue(requestedType, out var analyzer))
        {
            return analyzer;
        }

        throw new InvalidOperationException($"Analyzer '{requestedType}' is not registered.");
    }
}
