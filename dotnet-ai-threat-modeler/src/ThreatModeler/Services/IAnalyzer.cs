using ThreatModeler.Models;

namespace ThreatModeler.Services;

public interface IAnalyzer
{
    // Strategy pattern: each analyzer offers the same operation while varying the threat-modeling algorithm.
    string AnalyzerType { get; }
    Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default);
}

public interface IAnalyzerFactory
{
    // Factory Method pattern: callers request the analyzer they need without knowing concrete analyzer types.
    IAnalyzer Create(string analyzerType);
}
