using ThreatModeler.Models;

namespace ThreatModeler.Interfaces;

// Builder pattern: prompt assembly is isolated from context retrieval and chat-client transport.
public interface IPromptBuilder
{
    string Build(Submission submission, PromptContext context);
}
