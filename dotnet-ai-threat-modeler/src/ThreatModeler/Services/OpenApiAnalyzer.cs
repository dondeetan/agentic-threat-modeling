using System.Text.Json;
using ThreatModeler.Configuration;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class OpenApiAnalyzer : IAnalyzer
{
    public string AnalyzerType => AnalyzerTypes.OpenApi;

    public Task<object> AnalyzeAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        // Strategy: OpenAPI threat modeling is one interchangeable analyzer algorithm behind IAnalyzer.
        var endpoints = OpenApiEndpointCatalog.FromDocument(submission.OpenApiDocument);
        var components = BuildComponents(submission, endpoints);
        var threats = BuildThreats(submission, endpoints);

        var result = new
        {
            summary = endpoints.Count == 0
                ? $"{submission.ApplicationName} was analyzed with the OpenAPI strategy using the submitted architecture context."
                : $"{submission.ApplicationName} exposes {endpoints.Count} OpenAPI operations that should be threat modeled at the API boundary.",
            assets = components,
            trustBoundaries = submission.TrustBoundaries.DefaultIfEmpty("Client to API boundary").ToArray(),
            endpoints,
            threats,
            topPriorities = new[]
            {
                "Require strong authentication and authorization on every OpenAPI operation.",
                "Validate request bodies, route parameters, and query parameters against the OpenAPI contract.",
                "Apply rate limits, logging, and sensitive-data redaction at the API gateway and application layers."
            }
        };

        return Task.FromResult<object>(result);
    }

    private static IReadOnlyCollection<object> BuildComponents(Submission submission, IReadOnlyCollection<OpenApiEndpoint> endpoints)
    {
        var declaredComponents = submission.Components
            .DefaultIfEmpty("API")
            .Select(component => new { name = component, type = "component", source = "submission" })
            .Cast<object>()
            .ToList();

        declaredComponents.AddRange(endpoints
            .Select(endpoint => endpoint.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => new { name = path, type = "openapi-path", source = "openapi" }));

        return declaredComponents;
    }

    private static IReadOnlyCollection<object> BuildThreats(Submission submission, IReadOnlyCollection<OpenApiEndpoint> endpoints)
    {
        var primaryComponent = submission.Components.FirstOrDefault() ?? "API";
        var endpointList = endpoints.Take(5).Select(endpoint => $"{endpoint.Method} {endpoint.Path}").ToArray();

        return new object[]
        {
            new
            {
                id = "OAS-001",
                strideCategory = "Spoofing",
                component = primaryComponent,
                affectedOperations = endpointList,
                threatStatement = "Callers could impersonate users or services if OpenAPI operations do not consistently require validated identity.",
                mitigations = new[] { "Entra ID or OAuth2", "Validate issuer and audience", "Deny anonymous operations by default" }
            },
            new
            {
                id = "OAS-002",
                strideCategory = "Tampering",
                component = primaryComponent,
                affectedOperations = endpointList,
                threatStatement = "Requests could be altered or over-posted if handlers trust payloads beyond the OpenAPI schema.",
                mitigations = new[] { "Schema validation", "Allow-list fields", "Server-side authorization checks" }
            },
            new
            {
                id = "OAS-003",
                strideCategory = "Information Disclosure",
                component = primaryComponent,
                affectedOperations = endpointList,
                threatStatement = "Responses, errors, or logs could expose sensitive data described in the API contract.",
                mitigations = new[] { "Response filtering", "ProblemDetails without internals", "Log redaction" }
            }
        };
    }
}

internal static class OpenApiEndpointCatalog
{
    public static IReadOnlyCollection<OpenApiEndpoint> FromDocument(string document)
    {
        // Single Responsibility Principle: OpenAPI parsing is isolated from analyzer risk scoring.
        if (string.IsNullOrWhiteSpace(document))
        {
            return Array.Empty<OpenApiEndpoint>();
        }

        try
        {
            using var json = JsonDocument.Parse(document);
            if (!json.RootElement.TryGetProperty("paths", out var paths) || paths.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<OpenApiEndpoint>();
            }

            var endpoints = new List<OpenApiEndpoint>();
            foreach (var path in paths.EnumerateObject())
            {
                foreach (var operation in path.Value.EnumerateObject())
                {
                    if (IsHttpMethod(operation.Name))
                    {
                        endpoints.Add(new OpenApiEndpoint(operation.Name.ToUpperInvariant(), path.Name));
                    }
                }
            }

            return endpoints;
        }
        catch (JsonException)
        {
            return Array.Empty<OpenApiEndpoint>();
        }
    }

    private static bool IsHttpMethod(string name) =>
        name.Equals("get", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("post", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("put", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("patch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("delete", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("head", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("options", StringComparison.OrdinalIgnoreCase);
}

public sealed record OpenApiEndpoint(string Method, string Path);
