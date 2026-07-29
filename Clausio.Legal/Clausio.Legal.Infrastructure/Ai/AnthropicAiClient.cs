using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Clausio.Legal.Infrastructure.Ai;

public class AnthropicAiClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly string     _model;
    private readonly string     _apiKey;
    private readonly string     _baseUrl;

    public AnthropicAiClient(IConfiguration config)
    {
        _apiKey  = config["ClaudeApi:ApiKey"]  ?? throw new InvalidOperationException("ClaudeApi:ApiKey is not configured");
        _model   = config["ClaudeApi:Model"]   ?? "meta/llama-3.3-70b-instruct";
        _baseUrl = config["ClaudeApi:BaseUrl"] ?? "https://integrate.api.nvidia.com/v1";

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
        _http.Timeout = TimeSpan.FromSeconds(300); // 5 minutes
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
            temperature = 0.7,
            messages    = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPrompt   },
            }
        };

        var json    = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Use a new CancellationToken with 5 minute timeout
        // This ignores the ASP.NET request cancellation
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));

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