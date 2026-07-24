using System.Text.RegularExpressions;
using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/readiness")]
public partial class ReadinessController(IReadinessService readinessService, IAiService aiService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await readinessService.GetOrCreateAsync(caseId, cancellationToken));

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(Guid caseId, CancellationToken cancellationToken)
    {
        var raw = await aiService.AssessReadinessAsync(caseId, cancellationToken);
        var (score, checklist) = ParseAssessment(raw);
        return Ok(await readinessService.SetGeneratedAsync(caseId, score, checklist, cancellationToken));
    }

    [HttpPut("score")]
    public async Task<IActionResult> UpdateScore(Guid caseId, UpdateScoreDto dto, CancellationToken cancellationToken) =>
        Ok(await readinessService.UpdateScoreAsync(caseId, dto.Score, cancellationToken));

    [HttpGet("checklist")]
    public async Task<IActionResult> GetChecklist(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await readinessService.GetChecklistAsync(caseId, cancellationToken));

    private static (int Score, List<(string Text, string? Category)> Checklist) ParseAssessment(string raw)
    {
        var score = 50;
        var scoreMatch = ScoreRegex().Match(raw);
        if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out var parsed))
        {
            score = Math.Clamp(parsed, 0, 100);
        }

        var checklist = new List<(string, string?)>();
        foreach (Match line in ChecklistLineRegex().Matches(raw))
        {
            var category = line.Groups["category"].Success ? line.Groups["category"].Value : null;
            checklist.Add((line.Groups["text"].Value.Trim(), category));
        }

        return (score, checklist);
    }

    [GeneratedRegex(@"Score:\s*(\d{1,3})", RegexOptions.IgnoreCase)]
    private static partial Regex ScoreRegex();

    [GeneratedRegex(@"^-\s*(\[(?<category>[^\]]+)\]\s*)?(?<text>.+)$", RegexOptions.Multiline)]
    private static partial Regex ChecklistLineRegex();
}
