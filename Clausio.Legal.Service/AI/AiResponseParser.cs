using System.Text.Json;
using Clausio.Legal.Core.Dtos.AI;

namespace Clausio.Legal.Service.AI;

public class AiResponseParser
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CaseSummaryResponseDto ParseSummary(string json)
    {
        return Deserialize<CaseSummaryResponseDto>(json);
    }

    public ChronologyResponseDto ParseChronology(string json)
    {
        var events = Deserialize<List<ChronologyEventDto>>(json);

        return new ChronologyResponseDto
        {
            Events = events
        };
    }

    public EvidenceAnalysisResponseDto ParseEvidence(string json)
    {
        return Deserialize<EvidenceAnalysisResponseDto>(json);
    }

    private T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("AI returned an empty response.");

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, _options);

            if (result == null)
                throw new InvalidOperationException("AI returned invalid JSON.");

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"AI returned malformed JSON.\n\nResponse:\n{json}",
                ex);
        }
    }
}