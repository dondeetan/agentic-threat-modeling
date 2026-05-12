using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

namespace ChatClientShared.AzureOpenAIChatClientShared;

public static class AzureOpenAIChatClientFactory
{
    public static IChatClient Create(IConfiguration config)
    {
        AzureOpenAIChatClientSettings settings = AzureOpenAIChatClientSettings.FromConfiguration(config);

        return Create(settings);
    }

    public static IChatClient Create(AzureOpenAIChatClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new AzureOpenAIClient(settings.Endpoint, new ApiKeyCredential(settings.ApiKey))
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }
}
