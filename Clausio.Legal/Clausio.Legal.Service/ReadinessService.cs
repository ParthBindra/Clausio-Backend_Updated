using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IReadinessService
{
    Task<Readiness> GetOrCreateAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Readiness> UpdateScoreAsync(Guid caseId, int score, CancellationToken cancellationToken = default);
    Task<List<ReadinessChecklistItem>> GetChecklistAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Readiness> SetGeneratedAsync(Guid caseId, int score, List<(string Text, string? Category)> checklist, CancellationToken cancellationToken = default);
}

public class ReadinessService(ClausioDbContext db) : IReadinessService
{
    public async Task<Readiness> GetOrCreateAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var readiness = await db.Readinesses.Include(r => r.ChecklistItems)
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

    public async Task<Readiness> SetGeneratedAsync(Guid caseId, int score, List<(string Text, string? Category)> checklist, CancellationToken cancellationToken = default)
    {
        var readiness = await GetOrCreateAsync(caseId, cancellationToken);
        readiness.Score = score;

        db.ReadinessChecklistItems.RemoveRange(readiness.ChecklistItems);
        readiness.ChecklistItems = checklist
            .Select(item => new ReadinessChecklistItem { ReadinessId = readiness.Id, Text = item.Text, Category = item.Category })
            .ToList();

        await db.SaveChangesAsync(cancellationToken);
        return readiness;
    }
}
