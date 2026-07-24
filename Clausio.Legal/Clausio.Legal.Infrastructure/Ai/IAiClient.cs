namespace Clausio.Legal.Infrastructure.Ai;

public interface IAiClient
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
