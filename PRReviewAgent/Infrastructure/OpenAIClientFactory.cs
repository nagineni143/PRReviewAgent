using OpenAI;
using OpenAI.Chat;

public class OpenAIClientFactory
{
    private readonly ChatClient _chatClient;

    public OpenAIClientFactory(IConfiguration config)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        var client = new OpenAIClient(apiKey);

        _chatClient = client.GetChatClient("gpt-4o-mini");
    }

    public ChatClient GetChatClient()
    {
        return _chatClient;
    }
}