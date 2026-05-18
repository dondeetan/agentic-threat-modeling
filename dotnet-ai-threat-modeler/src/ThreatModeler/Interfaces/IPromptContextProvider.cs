using ThreatModeler.Models;

namespace ThreatModeler.Interfaces;

// Provider pattern: prompt retrieval is abstracted so file-backed rules can later be replaced by AI Search or vector retrieval.
public interface IPromptContextProvider
{
    Task<PromptContext> GetContextAsync(Submission submission, CancellationToken cancellationToken = default);
}
