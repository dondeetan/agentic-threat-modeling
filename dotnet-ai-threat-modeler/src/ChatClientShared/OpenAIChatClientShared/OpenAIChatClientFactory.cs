using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace ChatClientShared.OpenAIChatClientShared;

public static class OpenAIChatClientFactory
{
    public static IChatClient Create(IConfiguration config)
    {
        OpenAIChatClientSettings settings = OpenAIChatClientSettings.FromConfiguration(config);

        return Create(settings);
    }

    public static IChatClient Create(OpenAIChatClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        OpenAIClientOptions options = new()
        {
            Endpoint = settings.Endpoint
        };

        return new OpenAIClient(new ApiKeyCredential(settings.ApiKey), options)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }
}
