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

    public async Task<Readiness> SetGeneratedAsync(
        Guid caseId,
        int score,
        List<(string Text, string? Category)> checklist,
        List<GapItem> gaps,
        List<string> strengths,
        string summary,
        CancellationToken cancellationToken = default)
    {
        // ✅ FIX — fresh load with tracking to avoid concurrency errors
        var readiness = await db.Readinesses
            .Include(r => r.ChecklistItems)
            .FirstOrDefaultAsync(r => r.CaseId == caseId, cancellationToken);

        if (readiness is null)
        {
            readiness = new Readiness { CaseId = caseId };
            db.Readinesses.Add(readiness);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Update fields
        readiness.Score         = score;
        readiness.Summary       = summary;
        readiness.StrengthsJson = System.Text.Json.JsonSerializer.Serialize(strengths);
        readiness.GapsJson      = System.Text.Json.JsonSerializer.Serialize(gaps);

        // Remove old checklist items
        if (readiness.ChecklistItems.Any())
        {
            db.ReadinessChecklistItems.RemoveRange(readiness.ChecklistItems);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Add new checklist items
        var newItems = checklist.Select(item => new ReadinessChecklistItem
        {
            ReadinessId = readiness.Id,
            Text        = item.Text,
            Category    = item.Category,
        }).ToList();

        db.ReadinessChecklistItems.AddRange(newItems);
        await db.SaveChangesAsync(cancellationToken);

        // Reload with fresh data
        return await db.Readinesses
            .Include(r => r.ChecklistItems)
            .FirstAsync(r => r.CaseId == caseId, cancellationToken);
    }
}
