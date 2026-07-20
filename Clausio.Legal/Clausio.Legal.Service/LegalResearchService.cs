using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public record LegalResearchSummary(int Total, int StrongPrecedents);

public interface ILegalResearchService
{
    Task<List<LegalResearch>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<LegalResearch> CreateAsync(Guid caseId, CreateLegalResearchDto dto, CancellationToken cancellationToken = default);
    Task<List<LegalResearch>> CreateBulkAsync(Guid caseId, List<CreateLegalResearchDto> dtos, CancellationToken cancellationToken = default);
    Task<LegalResearch?> UpdateAsync(Guid caseId, Guid id, CreateLegalResearchDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
    Task<LegalResearchSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public class LegalResearchService(ClausioDbContext db) : ILegalResearchService
{
    public Task<List<LegalResearch>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.LegalResearches.AsNoTracking().Where(r => r.CaseId == caseId).OrderByDescending(r => r.Year).ToListAsync(cancellationToken);

    public async Task<LegalResearch> CreateAsync(Guid caseId, CreateLegalResearchDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Map(caseId, dto);
        db.LegalResearches.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<List<LegalResearch>> CreateBulkAsync(Guid caseId, List<CreateLegalResearchDto> dtos, CancellationToken cancellationToken = default)
    {
        var entities = dtos.Select(dto => Map(caseId, dto)).ToList();
        db.LegalResearches.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<LegalResearch?> UpdateAsync(Guid caseId, Guid id, CreateLegalResearchDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.LegalResearches.FirstOrDefaultAsync(r => r.CaseId == caseId && r.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.Citation = dto.Citation;
        entity.Court = dto.Court;
        entity.Year = dto.Year;
        entity.RatioDecidendi = dto.RatioDecidendi;
        entity.Relevance = dto.Relevance;
        entity.HowToUse = dto.HowToUse;
        entity.Strength = dto.Strength;
        entity.FullJudgmentUrl = dto.FullJudgmentUrl;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.LegalResearches.FirstOrDefaultAsync(r => r.CaseId == caseId && r.Id == id, cancellationToken);
        if (entity is null) return false;

        db.LegalResearches.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<LegalResearchSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var items = await db.LegalResearches.AsNoTracking().Where(r => r.CaseId == caseId).ToListAsync(cancellationToken);
        return new LegalResearchSummary(items.Count, items.Count(r => string.Equals(r.Strength, "Strong", StringComparison.OrdinalIgnoreCase)));
    }

    private static LegalResearch Map(Guid caseId, CreateLegalResearchDto dto) => new()
    {
        CaseId = caseId,
        Citation = dto.Citation,
        Court = dto.Court,
        Year = dto.Year,
        RatioDecidendi = dto.RatioDecidendi,
        Relevance = dto.Relevance,
        HowToUse = dto.HowToUse,
        Strength = dto.Strength,
        FullJudgmentUrl = dto.FullJudgmentUrl,
    };
}
