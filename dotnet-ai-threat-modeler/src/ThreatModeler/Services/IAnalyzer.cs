using ThreatModeler.Models;

namespace ThreatModeler.Services;

public interface IAnalyzer
{
    string AnalyzerType { get; }
    Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default);
}
