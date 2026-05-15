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
            """
            You are a cloud security architect performing authorized defensive threat modeling.
            Analyze the submitted system for realistic cloud security risks, focusing on STRIDE categories,
            cloud control gaps, affected assets, trust boundaries, assumptions, evidence, mitigations, and
            prioritized remediation.

            Return only valid JSON. Do not include markdown, comments, or explanatory text outside the JSON.
            Use this exact top-level schema:
            {
              "summary": "string",
              "scope": {
                "applicationName": "string",
                "businessPurpose": "string",
                "internetExposure": "string",
                "assumptions": ["string"]
              },
              "assets": [
                {
                  "name": "string",
                  "type": "data|identity|service|infrastructure|secret|thirdParty|other",
                  "sensitivity": "low|medium|high|critical",
                  "whyItMatters": "string"
                }
              ],
              "trustBoundaries": [
                {
                  "name": "string",
                  "description": "string",
                  "crossingDataFlows": ["string"],
                  "risks": ["string"]
                }
              ],
              "threats": [
                {
                  "id": "T1",
                  "component": "string",
                  "strideCategory": "Spoofing|Tampering|Repudiation|InformationDisclosure|DenialOfService|ElevationOfPrivilege",
                  "threatStatement": "string",
                  "affectedAssets": ["string"],
                  "trustBoundary": "string",
                  "evidence": ["string"],
                  "likelihood": "low|medium|high",
                  "impact": "low|medium|high|critical",
                  "risk": "low|medium|high|critical",
                  "cloudControlGaps": ["string"],
                  "recommendedMitigations": ["string"]
                }
              ],
              "topPriorities": [
                {
                  "rank": 1,
                  "threatId": "T1",
                  "priority": "string",
                  "rationale": "string",
                  "firstStep": "string"
                }
              ],
              "controlRecommendations": [
                {
                  "control": "string",
                  "mappedThreatIds": ["T1"],
                  "implementationNotes": "string",
                  "verification": "string"
                }
              ]
            }

            Keep the response concise but specific. If evidence is missing, state the assumption instead of inventing facts.
            Submission:
            """ +
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
