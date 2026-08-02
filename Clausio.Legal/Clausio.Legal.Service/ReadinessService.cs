using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IReadinessService
{
    Task<Readiness> GetOrCreateAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Readiness> UpdateScoreAsync(Guid caseId, int score, CancellationToken cancellationToken = default);
    Task<List<ReadinessChecklistItem>> GetChecklistAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Readiness> SetGeneratedAsync(
        Guid caseId,
        int score,
        List<(string Text, string? Category)> checklist,
        List<GapItem> gaps,
        List<string> strengths,
        string summary,
        CancellationToken cancellationToken = default);
}

// ✅ GapItem — matches AI JSON output format
public record GapItem(string Title, string Description, string Severity, bool Resolved);

public class ReadinessService(ClausioDbContext db) : IReadinessService
{
    public async Task<Readiness> GetOrCreateAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var readiness = await db.Readinesses
            .Include(r => r.ChecklistItems)
            .FirstOrDefaultAsync(r => r.CaseId == caseId, cancellationToken);

        if (readiness is not null) return readiness;

        readiness = new Readiness { CaseId = caseId, Score = 0 };
        db.Readinesses.Add(readiness);
        await db.SaveChangesAsync(cancellationToken);
        return readiness;
    }

    public async Task<Readiness> UpdateScoreAsync(Guid caseId, int score, CancellationToken cancellationToken = default)
    {
        var readiness = await GetOrCreateAsync(caseId, cancellationToken);
        readiness.Score = score;
        await db.SaveChangesAsync(cancellationToken);
        return readiness;
    }

    public async Task<List<ReadinessChecklistItem>> GetChecklistAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var readiness = await GetOrCreateAsync(caseId, cancellationToken);
        return readiness.ChecklistItems.ToList();
    }

    // ✅ Updated — now stores gaps, strengths, and summary from AI JSON response
    public async Task<Readiness> SetGeneratedAsync(
        Guid caseId,
        int score,
        List<(string Text, string? Category)> checklist,
        List<GapItem> gaps,
        List<string> strengths,
        string summary,
        CancellationToken cancellationToken = default)
    {
        var readiness = await GetOrCreateAsync(caseId, cancellationToken);
        readiness.Score   = score;
        readiness.Summary = summary;

        // Save strengths as JSON string
        readiness.StrengthsJson = System.Text.Json.JsonSerializer.Serialize(strengths);

        // Save gaps as JSON string
        readiness.GapsJson = System.Text.Json.JsonSerializer.Serialize(gaps);

        // Update checklist items
        db.ReadinessChecklistItems.RemoveRange(readiness.ChecklistItems);
        readiness.ChecklistItems = checklist
            .Select(item => new ReadinessChecklistItem
            {
                ReadinessId = readiness.Id,
                Text        = item.Text,
                Category    = item.Category,
            })
            .ToList();

        await db.SaveChangesAsync(cancellationToken);
        return readiness;
    }
}
