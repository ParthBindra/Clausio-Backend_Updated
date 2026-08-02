using System.Text.Json;
using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/readiness")]
public class ReadinessController(IReadinessService readinessService, IAiService aiService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await readinessService.GetOrCreateAsync(caseId, cancellationToken));

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(Guid caseId, CancellationToken cancellationToken)
    {
        var raw = await aiService.AssessReadinessAsync(caseId, cancellationToken);
        var (score, checklist, gaps, strengths, summary) = ParseAssessment(raw);
        var result = await readinessService.SetGeneratedAsync(caseId, score, checklist, gaps, strengths, summary, cancellationToken);
        return Ok(result);
    }

    [HttpPut("score")]
    public async Task<IActionResult> UpdateScore(Guid caseId, UpdateScoreDto dto, CancellationToken cancellationToken) =>
        Ok(await readinessService.UpdateScoreAsync(caseId, dto.Score, cancellationToken));

    [HttpGet("checklist")]
    public async Task<IActionResult> GetChecklist(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await readinessService.GetChecklistAsync(caseId, cancellationToken));

    private static (int Score, List<(string Text, string? Category)> Checklist, List<GapItem> Gaps, List<string> Strengths, string Summary) ParseAssessment(string raw)
    {
        var score     = 50;
        var checklist = new List<(string, string?)>();
        var gaps      = new List<GapItem>();
        var strengths = new List<string>();
        var summary   = "";

        try
        {
            var cleaned = raw.Trim();
            if (cleaned.StartsWith("```")) cleaned = cleaned.Substring(cleaned.IndexOf('\n') + 1);
            if (cleaned.EndsWith("```"))  cleaned = cleaned.Substring(0, cleaned.LastIndexOf("```"));
            cleaned = cleaned.Trim();

            var doc  = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            if (root.TryGetProperty("score", out var scoreProp) && scoreProp.TryGetInt32(out var parsedScore))
                score = Math.Clamp(parsedScore, 0, 100);

            if (root.TryGetProperty("summary", out var summaryProp))
                summary = summaryProp.GetString() ?? "";

            if (root.TryGetProperty("strengths", out var strengthsProp) && strengthsProp.ValueKind == JsonValueKind.Array)
                foreach (var s in strengthsProp.EnumerateArray())
                {
                    var str = s.GetString();
                    if (!string.IsNullOrEmpty(str)) strengths.Add(str);
                }

            if (root.TryGetProperty("gaps", out var gapsProp) && gapsProp.ValueKind == JsonValueKind.Array)
                foreach (var g in gapsProp.EnumerateArray())
                {
                    var title       = g.TryGetProperty("title",       out var t)  ? t.GetString()  ?? "" : "";
                    var description = g.TryGetProperty("description", out var d)  ? d.GetString()  ?? "" : "";
                    var severity    = g.TryGetProperty("severity",    out var sv) ? sv.GetString() ?? "Medium" : "Medium";
                    var resolved    = g.TryGetProperty("resolved",    out var r)  && r.GetBoolean();
                    if (!string.IsNullOrEmpty(title))
                    {
                        gaps.Add(new GapItem(title, description, severity, resolved));
                        checklist.Add((title, severity));
                    }
                }

            if (root.TryGetProperty("nextSteps", out var nextStepsProp) && nextStepsProp.ValueKind == JsonValueKind.Array)
                foreach (var step in nextStepsProp.EnumerateArray())
                {
                    var stepText = step.GetString();
                    if (!string.IsNullOrEmpty(stepText)) checklist.Add((stepText, "Action"));
                }
        }
        catch
        {
            foreach (var line in raw.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Score:", StringComparison.OrdinalIgnoreCase))
                {
                    var numStr = trimmed["Score:".Length..].Trim();
                    if (int.TryParse(numStr.Split(' ')[0], out var s))
                        score = Math.Clamp(s, 0, 100);
                }
                else if (trimmed.StartsWith("- "))
                    checklist.Add((trimmed[2..].Trim(), null));
            }
        }

        return (score, checklist, gaps, strengths, summary);
    }
}
