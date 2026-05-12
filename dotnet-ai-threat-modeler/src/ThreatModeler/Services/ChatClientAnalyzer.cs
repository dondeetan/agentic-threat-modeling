using System.Text.Json;
using Microsoft.Extensions.AI;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class ChatClientAnalyzer : IAnalyzer
{
    private readonly Lazy<IChatClient> _chatClient;

    public ChatClientAnalyzer(string analyzerType, Func<IChatClient> chatClientFactory)
    {
        AnalyzerType = analyzerType;
        _chatClient = new Lazy<IChatClient>(chatClientFactory);
    }

    public string AnalyzerType { get; }

    public async Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        var response = await _chatClient.Value.GetResponseAsync(
            BuildPrompt(submission),
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

    private static string BuildPrompt(Submission submission)
    {
        var payload = JsonSerializer.Serialize(submission, new JsonSerializerOptions { WriteIndented = true });

        return
            "You are a cloud security architect. Create a concise threat model from the submission. " +
            "Return only valid JSON with summary, assets, trustBoundaries, threats, topPriorities, and controlRecommendations." +
            Environment.NewLine +
            payload;
    }

    private static JsonElement? TryReadJson(string text)
    {
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
