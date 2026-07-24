using Anthropic;
using Anthropic.Models.Messages;

namespace Clausio.Legal.Infrastructure.Ai;

public class AnthropicAiClient : IAiClient
{
    private readonly AnthropicClient _client = new();

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = Model.ClaudeOpus4_8,
            MaxTokens = 4096,
            System = systemPrompt,
            Thinking = new ThinkingConfigAdaptive(),
            Messages = [new() { Role = Role.User, Content = userPrompt }],
        }, cancellationToken: cancellationToken);

        return string.Concat(response.Content
            .Select(block => block.Value)
            .OfType<TextBlock>()
            .Select(text => text.Text));
    }
}
