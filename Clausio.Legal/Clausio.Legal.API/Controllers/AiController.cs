using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
public class AiController(IAiService aiService) : ControllerBase
{
    [HttpPost("summary/{caseId:guid}")]
    public async Task<IActionResult> Summary(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.SummarizeCaseAsync(caseId, cancellationToken) });

    [HttpPost("chronology/{caseId:guid}")]
    public async Task<IActionResult> Chronology(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.GenerateChronologyAsync(caseId, cancellationToken) });

    [HttpPost("contradictions/{caseId:guid}")]
    public async Task<IActionResult> Contradictions(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.DetectContradictionsAsync(caseId, cancellationToken) });

    [HttpPost("evidence/{documentId:guid}")]
    public async Task<IActionResult> Evidence(Guid documentId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.AnalyzeEvidenceAsync(documentId, cancellationToken) });

    [HttpPost("research/{caseId:guid}")]
    public async Task<IActionResult> Research(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.ResearchAsync(caseId, cancellationToken) });

    [HttpPost("actionplan/{caseId:guid}")]
    public async Task<IActionResult> ActionPlan(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.GenerateActionPlanAsync(caseId, cancellationToken) });

    [HttpPost("translate")]
    public async Task<IActionResult> Translate(TranslateRequest request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.TranslateAsync(request, cancellationToken) });

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(ChatRequestDto request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.ChatAsync(request, cancellationToken) });

    [HttpPost("whatsapp/{caseId:guid}")]
    public async Task<IActionResult> WhatsApp(Guid caseId, WhatsAppRequestDto request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.DraftWhatsAppAsync(caseId, request, cancellationToken) });

    [HttpPost("financial/{caseId:guid}")]
    public async Task<IActionResult> Financial(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.AnalyzeFinancialsAsync(caseId, cancellationToken) });

    [HttpPost("readiness/{caseId:guid}")]
    public async Task<IActionResult> Readiness(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.AssessReadinessAsync(caseId, cancellationToken) });

    [HttpPost("emergency/{caseId:guid}")]
    public async Task<IActionResult> Emergency(Guid caseId, EmergencyRequestDto request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.EmergencyTriageAsync(request, cancellationToken) });

    [HttpPost("prep/{caseId:guid}")]
    public async Task<IActionResult> Prep(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.PrepHearingAsync(caseId, cancellationToken) });

    [HttpPost("witness/{caseId:guid}")]
    public async Task<IActionResult> Witness(Guid caseId, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.PrepWitnessAsync(caseId, cancellationToken) });

    [HttpPost("casetype")]
    public async Task<IActionResult> CaseType(CaseTypeRequestDto request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.ClassifyCaseTypeAsync(request, cancellationToken) });

    [HttpPost("draft/{caseId:guid}")]
    public async Task<IActionResult> Draft(Guid caseId, DraftRequestDto request, CancellationToken cancellationToken) =>
        Ok(new AiResultDto { Result = await aiService.DraftDocumentAsync(caseId, request, cancellationToken) });
}
