using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace ChatClientShared.OpenAIChatClientShared;

public static class OpenAIChatClientFactory
{
    public static IChatClient Create(IConfiguration config)
    {
        // Factory Method pattern: configuration is translated into a ready-to-use chat client.
        OpenAIChatClientSettings settings = OpenAIChatClientSettings.FromConfiguration(config);

        return Create(settings);
    }

    public static IChatClient Create(OpenAIChatClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Adapter pattern: the OpenAI SDK chat client is adapted to Microsoft.Extensions.AI.IChatClient.
        OpenAIClientOptions options = new()
        {
            Endpoint = settings.Endpoint
        };

        return new OpenAIClient(new ApiKeyCredential(settings.ApiKey), options)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }
}
