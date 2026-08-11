using System.Text.Json;
using Microsoft.Extensions.AI;
using ThreatModeler.Interfaces;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class ChatClientAnalyzer : IAnalyzer
{
    // Lazy initialization defers external client creation until this strategy is actually selected.
    private readonly Lazy<IChatClient> _chatClient;
    private readonly IPromptContextProvider _promptContextProvider;
    private readonly IPromptBuilder _promptBuilder;

    public ChatClientAnalyzer(
        string analyzerType,
        Func<IChatClient> chatClientFactory,
        IPromptContextProvider? promptContextProvider = null,
        IPromptBuilder? promptBuilder = null)
    {
        AnalyzerType = analyzerType;
        _chatClient = new Lazy<IChatClient>(chatClientFactory);
        _promptContextProvider = promptContextProvider ?? new FilePromptContextProvider();
        _promptBuilder = promptBuilder ?? new PromptBuilder();
    }

    public string AnalyzerType { get; }

    public async Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        // RAG-style context provider: analyzer asks for prompt context without knowing how it is retrieved.
        var promptContext = await _promptContextProvider.GetContextAsync(submission, cancellationToken);

        // Strategy pattern: this implementation delegates analysis to a configured chat client.
        var promptBuilderResult =  _promptBuilder.Build(submission, promptContext);
        var response = await _chatClient.Value.GetResponseAsync(
            promptBuilderResult,
            cancellationToken: cancellationToken);

        var json = TryReadJson(response.Text);
        if (json is not null)
        {
            return json.Value;
        }

        return new
        {
            summary = $"{AnalyzerType} returned an unstructured threat model response.",
            analyzer = AnalyzerType,
            result = response.Text
        };
    }

    private static JsonElement? TryReadJson(string text)
    {
        // Adapter-style normalization: structured model output is returned as JSON, otherwise wrapped consistently.
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
