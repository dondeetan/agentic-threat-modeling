using Microsoft.Extensions.Configuration;

namespace ChatClientShared.AzureOpenAIChatClientShared;

public sealed record AzureOpenAIChatClientSettings(Uri Endpoint, string ApiKey, string Model)
{
    // Options pattern: one immutable settings record carries the Azure OpenAI client configuration.
    public const string ConfigurationSectionName = "AzureOpenAI";
    public const string DefaultModel = "gpt-5-mini";

    public static AzureOpenAIChatClientSettings FromConfiguration(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        IConfigurationSection section = config.GetSection(ConfigurationSectionName);
        string endpoint = section["Endpoint"]
            ?? throw new InvalidOperationException("Set AzureOpenAI:Endpoint in appsettings.local.json. See appsettings.local.json.example.");
        string apiKey = section["ApiKey"]
            ?? throw new InvalidOperationException("Set AzureOpenAI:ApiKey in appsettings.local.json. See appsettings.local.json.example.");
        string model = section["Model"] ?? DefaultModel;

        return new AzureOpenAIChatClientSettings(new Uri(endpoint), apiKey, model);
    }
}
