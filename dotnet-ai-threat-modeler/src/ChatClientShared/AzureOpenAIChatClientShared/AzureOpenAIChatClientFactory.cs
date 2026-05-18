using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

namespace ChatClientShared.AzureOpenAIChatClientShared;

public static class AzureOpenAIChatClientFactory
{
    public static IChatClient Create(IConfiguration config)
    {
        // Factory Method pattern: configuration is translated into a ready-to-use Azure OpenAI chat client.
        AzureOpenAIChatClientSettings settings = AzureOpenAIChatClientSettings.FromConfiguration(config);

        return Create(settings);
    }

    public static IChatClient Create(AzureOpenAIChatClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Adapter pattern: the Azure OpenAI SDK chat client is adapted to Microsoft.Extensions.AI.IChatClient.
        return new AzureOpenAIClient(settings.Endpoint, new ApiKeyCredential(settings.ApiKey))
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }
}
