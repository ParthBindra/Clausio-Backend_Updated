using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Clausio.Legal.Infrastructure.Ai;

public class AnthropicAiClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly string     _model;
    private readonly string     _baseUrl;

    public AnthropicAiClient(IConfiguration config)
    {
        var apiKey = config["ClaudeApi:ApiKey"]
            ?? throw new InvalidOperationException("ClaudeApi:ApiKey is not configured");
        _model   = config["ClaudeApi:Model"]   ?? "llama-3.3-70b-versatile";
        _baseUrl = config["ClaudeApi:BaseUrl"] ?? "https://api.groq.com/openai/v1";

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _http.Timeout = TimeSpan.FromSeconds(120);
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model       = _model,
            max_tokens  = 2048,
            temperature = 0.1,
            messages    = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPrompt   },
            }
        };

        var json    = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

        var response = await _http.PostAsync(
            $"{_baseUrl}/chat/completions",
            content,
            cts.Token);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
        var parsed       = JsonDocument.Parse(responseJson);

        return parsed.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
