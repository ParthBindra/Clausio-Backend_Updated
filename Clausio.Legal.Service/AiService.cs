using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Clausio.Legal.Infrastructure.Ai;
using Clausio.Legal.Service.AI;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Clausio.Legal.Service;

public interface IAiService
{
    Task<string> SummarizeCaseAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> GenerateChronologyAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> DetectContradictionsAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> AnalyzeEvidenceAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<string> ResearchAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> GenerateActionPlanAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default);
    Task<string> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
    Task<string> DraftWhatsAppAsync(Guid caseId, WhatsAppRequestDto request, CancellationToken cancellationToken = default);
    Task<string> AnalyzeFinancialsAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> AssessReadinessAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> EmergencyTriageAsync(EmergencyRequestDto request, CancellationToken cancellationToken = default);
    Task<string> PrepHearingAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> PrepWitnessAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> ClassifyCaseTypeAsync(CaseTypeRequestDto request, CancellationToken cancellationToken = default);
    Task<string> DraftDocumentAsync(Guid caseId, DraftRequestDto request, CancellationToken cancellationToken = default);
}

public class AiService(ClausioDbContext db, IAiClient aiClient, AiResponseParser parser) : IAiService
{
    private const string BaseSystemPrompt =
        "You are Clausio, an AI legal assistant embedded in a case-management system for litigators. " +
        "Be precise, cite facts from the provided case dossier only, and flag when information is missing. " +
        "Respond in clear, well-structured plain text suitable for direct display in a legal case file.";

    public Task<string> SummarizeCaseAsync(
    Guid caseId,
    CancellationToken cancellationToken = default) =>
    RunWithCaseContextAsync(
        caseId,
        PromptTemplates.JsonRules + "\n\n" + PromptTemplates.CaseSummary,
        cancellationToken);

    public Task<string> GenerateChronologyAsync(
        Guid caseId,
        CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(
            caseId,
            PromptTemplates.JsonRules + "\n\n" + PromptTemplates.Chronology,
            cancellationToken);

    public Task<string> DetectContradictionsAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Identify contradictions between claims and evidence in this case. For each, state the claim, the contradicting evidence, and how strong the contradiction is.", cancellationToken);

    public async Task<string> AnalyzeEvidenceAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await db.Documents.Include(d => d.Case).FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var prompt =
    PromptTemplates.JsonRules +
    "\n\n" +
    PromptTemplates.Evidence +
    "\n\n" +
    $"Case Name: {document.Case?.Name}\n\n" +
    BuildDocumentBlock(document);
        return await aiClient.CompleteAsync(BaseSystemPrompt, prompt, cancellationToken);
    }

    public Task<string> ResearchAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Suggest relevant case law and statutory citations that would support this case. For each, give the citation, its ratio decidendi, and how to use it.", cancellationToken);

    public Task<string> GenerateActionPlanAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Propose a prioritized action plan for this case: concrete next steps, who should own each, and a reasonable due date.", cancellationToken);

    public Task<string> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            "You are a professional legal translator. Detect the source language and translate the given text to English, preserving legal terminology and meaning.",
            request.Text ?? string.Empty,
            cancellationToken);

    public async Task<string> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = BaseSystemPrompt;
        if (request.CaseId is Guid caseId)
        {
            systemPrompt += "\n\n" + await BuildCaseDossierAsync(caseId, cancellationToken);
        }

        var sb = new StringBuilder();
        if (request.History is { Count: > 0 })
        {
            for (var i = 0; i < request.History.Count; i++)
            {
                sb.AppendLine($"{(i % 2 == 0 ? "User" : "Assistant")}: {request.History[i]}");
            }
        }
        sb.Append("User: ").Append(request.Message);

        return await aiClient.CompleteAsync(systemPrompt, sb.ToString(), cancellationToken);
    }

    public Task<string> DraftWhatsAppAsync(Guid caseId, WhatsAppRequestDto request, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(
            caseId,
            $"Draft a WhatsApp message to the client updating them on this case. Tone: {request.Tone ?? "professional"}. Language: {request.Language ?? "English"}. Keep it short and clear for a non-lawyer.",
            cancellationToken);

    public Task<string> AnalyzeFinancialsAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Analyze the financial aspects of this case (claims, damages, client's stated income/assets) and flag anything relevant to case strategy.", cancellationToken);

    public async Task<string> AssessReadinessAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        await RunWithCaseContextAsync(
            caseId,
            "Assess how ready this case is for the next hearing. Give a readiness score from 0-100 on the first line as \"Score: N\", " +
            "then a checklist of outstanding items, one per line, formatted as \"- [category] item\".",
            cancellationToken);

    public Task<string> EmergencyTriageAsync(EmergencyRequestDto request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            "You are a legal emergency triage assistant. Given an urgent query, assess severity, identify immediate legal risks, and give clear, actionable first steps.",
            request.Query ?? string.Empty,
            cancellationToken);

    public Task<string> PrepHearingAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Prepare a hearing briefing: key arguments to raise, anticipated opposing arguments, and questions to be ready for from the bench.", cancellationToken);

    public Task<string> PrepWitnessAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId, "Prepare witness examination notes for this case: likely questions, points to establish, and pitfalls to avoid under cross-examination.", cancellationToken);

    public Task<string> ClassifyCaseTypeAsync(CaseTypeRequestDto request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            "You classify legal matters. Given a description, respond with the most likely case type and sub-type, and a one-sentence justification.",
            request.Description ?? string.Empty,
            cancellationToken);

    public Task<string> DraftDocumentAsync(Guid caseId, DraftRequestDto request, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(
            caseId,
            $"Draft a {request.DraftType ?? "legal document"} for this case. Instructions: {request.Instructions}",
            cancellationToken);

    private async Task<string> RunWithCaseContextAsync(Guid caseId, string instruction, CancellationToken cancellationToken)
    {
        var dossier = await BuildCaseDossierAsync(caseId, cancellationToken);
        var systemPrompt = BaseSystemPrompt + "\n\n" + dossier;
        return await aiClient.CompleteAsync(systemPrompt, instruction, cancellationToken);
    }

    private async Task<string> BuildCaseDossierAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var caseEntity = await db.Cases
            .Include(c => c.Client)
            .Include(c => c.Hearings).ThenInclude(h => h.Orders)
            .Include(c => c.TimelineEvents)
            .Include(c => c.Contradictions)
            .Include(c => c.LegalResearches)
            .Include(c => c.ActionPlans)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        var sb = new StringBuilder();
        sb.AppendLine("CASE DOSSIER");
        sb.AppendLine($"Name: {caseEntity.Name}; Number: {caseEntity.CaseNumber}; Type: {caseEntity.CaseType}/{caseEntity.SubType}");
        sb.AppendLine($"Court: {caseEntity.Court} ({caseEntity.CourtLocation}); Stage: {caseEntity.Stage}; Status: {caseEntity.Status}; Priority: {caseEntity.Priority}");
        sb.AppendLine($"Client: {caseEntity.Client?.FirstName} {caseEntity.Client?.LastName}");
        sb.AppendLine($"Opposing advocate: {caseEntity.OpposingAdv}");
        sb.AppendLine($"Filed on: {caseEntity.FiledOn:d}; Next hearing: {caseEntity.NextHearing:d}");

        if (caseEntity.TimelineEvents.Count > 0)
        {
            sb.AppendLine("\nTIMELINE:");
            foreach (var e in caseEntity.TimelineEvents.OrderBy(e => e.SortOrder))
            {
                sb.AppendLine($"- {e.EventDate:d}: {e.Event} (source: {e.Source}; significance: {e.LegalSignificance})");
            }
        }

        if (caseEntity.Contradictions.Count > 0)
        {
            sb.AppendLine("\nCONTRADICTIONS:");
            foreach (var c in caseEntity.Contradictions)
            {
                sb.AppendLine($"- Claim: {c.Claim} ({c.ClaimSource}) vs Evidence: {c.Evidence} ({c.EvidenceSource})");
            }
        }

        if (caseEntity.LegalResearches.Count > 0)
        {
            sb.AppendLine("\nLEGAL RESEARCH:");
            foreach (var r in caseEntity.LegalResearches)
            {
                sb.AppendLine($"- {r.Citation} ({r.Court}, {r.Year}): {r.RatioDecidendi}");
            }
        }

        if (caseEntity.Hearings.Count > 0)
        {
            sb.AppendLine("\nHEARINGS:");
            foreach (var h in caseEntity.Hearings.OrderBy(h => h.HearingDate))
            {
                sb.AppendLine($"- {h.HearingDate:d}: {h.WhatHappened} (judge observation: {h.JudgeObservation})");
            }
        }

        if (caseEntity.ActionPlans.Count > 0)
        {
            sb.AppendLine("\nACTION PLANS:");
            foreach (var a in caseEntity.ActionPlans)
            {
                sb.AppendLine($"- [{(a.Done ? "done" : "pending")}] {a.Title} (due {a.DueBy:d})");
            }
        }

        if (caseEntity.Documents.Count > 0)
        {
            sb.AppendLine("\nDOCUMENTS ON FILE:");
            foreach (var d in caseEntity.Documents)
            {
                sb.AppendLine(BuildDocumentBlock(d));
            }
        }

        return sb.ToString();
    }

    private static string BuildDocumentBlock(Document document)
    {
        var sb = new StringBuilder();
        sb.AppendLine("--------------------------------");
        sb.AppendLine("DOCUMENT");
        sb.AppendLine(document.FileName);
        sb.AppendLine("TYPE");
        sb.AppendLine(document.DocumentType ?? "Unspecified");
        if (!string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            sb.AppendLine("CONTENT");
            sb.AppendLine(document.ExtractedText);
        }
        sb.AppendLine("--------------------------------");
        return sb.ToString();
    }
}