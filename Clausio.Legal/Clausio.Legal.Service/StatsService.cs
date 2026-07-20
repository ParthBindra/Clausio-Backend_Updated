using Clausio.Legal.Cache;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public record OverviewStats(int TotalCases, int ActiveCases, int TotalClients, int UpcomingHearings, double AverageReadinessScore);
public record CaseStats(Dictionary<string, int> ByStage, Dictionary<string, int> ByStatus, Dictionary<string, int> ByType);
public record HearingStats(int TotalHearings, int UpcomingHearings, int PastHearings);
public record DocumentStats(int TotalDocuments, long TotalSizeBytes, Dictionary<string, int> ByType);
public record ActivityItem(string Type, string Description, DateTime OccurredAt);

public interface IStatsService
{
    Task<OverviewStats> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<CaseStats> GetCaseStatsAsync(CancellationToken cancellationToken = default);
    Task<HearingStats> GetHearingStatsAsync(CancellationToken cancellationToken = default);
    Task<DocumentStats> GetDocumentStatsAsync(CancellationToken cancellationToken = default);
    Task<List<ActivityItem>> GetActivityAsync(CancellationToken cancellationToken = default);
}

public class StatsService(ClausioDbContext db, ICacheService cache) : IStatsService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    public async Task<OverviewStats> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGet("stats:overview", out OverviewStats? cached) && cached is not null) return cached;

        var now = DateTime.UtcNow;
        var totalCases = await db.Cases.CountAsync(cancellationToken);
        var activeCases = await db.Cases.CountAsync(c => c.Status == "Active", cancellationToken);
        var totalClients = await db.Clients.CountAsync(cancellationToken);
        var upcomingHearings = await db.Hearings.CountAsync(h => h.HearingDate >= now, cancellationToken);
        var avgReadiness = await db.Cases.Where(c => c.ReadinessScore != null).AverageAsync(c => (double?)c.ReadinessScore, cancellationToken) ?? 0;

        var result = new OverviewStats(totalCases, activeCases, totalClients, upcomingHearings, avgReadiness);
        cache.Set("stats:overview", result, Ttl);
        return result;
    }

    public async Task<CaseStats> GetCaseStatsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGet("stats:cases", out CaseStats? cached) && cached is not null) return cached;

        var byStage = await db.Cases.Where(c => c.Stage != null)
            .GroupBy(c => c.Stage!).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var byStatus = await db.Cases.Where(c => c.Status != null)
            .GroupBy(c => c.Status!).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var byType = await db.Cases.Where(c => c.CaseType != null)
            .GroupBy(c => c.CaseType!).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var result = new CaseStats(byStage, byStatus, byType);
        cache.Set("stats:cases", result, Ttl);
        return result;
    }

    public async Task<HearingStats> GetHearingStatsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGet("stats:hearings", out HearingStats? cached) && cached is not null) return cached;

        var now = DateTime.UtcNow;
        var total = await db.Hearings.CountAsync(cancellationToken);
        var upcoming = await db.Hearings.CountAsync(h => h.HearingDate >= now, cancellationToken);

        var result = new HearingStats(total, upcoming, total - upcoming);
        cache.Set("stats:hearings", result, Ttl);
        return result;
    }

    public async Task<DocumentStats> GetDocumentStatsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGet("stats:documents", out DocumentStats? cached) && cached is not null) return cached;

        var total = await db.Documents.CountAsync(cancellationToken);
        var totalSize = await db.Documents.SumAsync(d => (long?)d.SizeBytes, cancellationToken) ?? 0;
        var byType = await db.Documents.Where(d => d.DocumentType != null)
            .GroupBy(d => d.DocumentType!).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var result = new DocumentStats(total, totalSize, byType);
        cache.Set("stats:documents", result, Ttl);
        return result;
    }

    public async Task<List<ActivityItem>> GetActivityAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGet("stats:activity", out List<ActivityItem>? cached) && cached is not null) return cached;

        var recentCases = await db.Cases.OrderByDescending(c => c.CreatedAt).Take(10)
            .Select(c => new ActivityItem("Case", $"Case \"{c.Name}\" created", c.CreatedAt)).ToListAsync(cancellationToken);
        var recentHearings = await db.Hearings.OrderByDescending(h => h.CreatedAt).Take(10)
            .Select(h => new ActivityItem("Hearing", $"Hearing recorded for {h.HearingDate:d}", h.CreatedAt)).ToListAsync(cancellationToken);
        var recentDocuments = await db.Documents.OrderByDescending(d => d.CreatedAt).Take(10)
            .Select(d => new ActivityItem("Document", $"Document \"{d.FileName}\" uploaded", d.CreatedAt)).ToListAsync(cancellationToken);

        var result = recentCases.Concat(recentHearings).Concat(recentDocuments)
            .OrderByDescending(a => a.OccurredAt)
            .Take(20)
            .ToList();

        cache.Set("stats:activity", result, Ttl);
        return result;
    }
}
