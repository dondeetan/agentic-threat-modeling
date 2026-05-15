using Microsoft.Extensions.Configuration;

namespace ChatClientShared.OpenAIChatClientShared;

public sealed record OpenAIChatClientSettings(Uri Endpoint, string ApiKey, string Model)
{
    // Options pattern: one immutable settings record carries the OpenAI client configuration.
    public const string ConfigurationSectionName = "OpenAI";
    public const string DefaultEndpoint = "https://api.openai.com/v1";
    public const string DefaultModel = "gpt-5-mini";

    public static OpenAIChatClientSettings FromConfiguration(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        IConfigurationSection section = config.GetSection(ConfigurationSectionName);
        string endpoint = section["Endpoint"] ?? DefaultEndpoint;
        string apiKey = section["ApiKey"]
            ?? throw new InvalidOperationException("Set OpenAI:ApiKey in appsettings.local.json. See appsettings.local.json.example.");
        string model = section["Model"] ?? DefaultModel;

        if (endpoint.Contains("/responses", StringComparison.OrdinalIgnoreCase) ||
            endpoint.Contains("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OpenAI:Endpoint must be the base OpenAI REST API endpoint, such as https://api.openai.com/v1. Do not include /responses or /chat/completions.");
        }

        return new OpenAIChatClientSettings(new Uri(endpoint), apiKey, model);
    }
}
