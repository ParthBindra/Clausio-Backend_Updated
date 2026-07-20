using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public record ContradictionSummary(int Total, int Used, int Unused);

public interface IContradictionService
{
    Task<List<Contradiction>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Contradiction> CreateAsync(Guid caseId, CreateContradictionDto dto, CancellationToken cancellationToken = default);
    Task<List<Contradiction>> CreateBulkAsync(Guid caseId, List<CreateContradictionDto> dtos, CancellationToken cancellationToken = default);
    Task<Contradiction?> UpdateAsync(Guid caseId, Guid id, CreateContradictionDto dto, CancellationToken cancellationToken = default);
    Task<Contradiction?> SetUsedAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
    Task<ContradictionSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public class ContradictionService(ClausioDbContext db) : IContradictionService
{
    public Task<List<Contradiction>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.Contradictions.AsNoTracking().Where(c => c.CaseId == caseId).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Contradiction> CreateAsync(Guid caseId, CreateContradictionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Map(caseId, dto);
        db.Contradictions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<List<Contradiction>> CreateBulkAsync(Guid caseId, List<CreateContradictionDto> dtos, CancellationToken cancellationToken = default)
    {
        var entities = dtos.Select(dto => Map(caseId, dto)).ToList();
        db.Contradictions.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<Contradiction?> UpdateAsync(Guid caseId, Guid id, CreateContradictionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Contradictions.FirstOrDefaultAsync(c => c.CaseId == caseId && c.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.Claim = dto.Claim;
        entity.ClaimSource = dto.ClaimSource;
        entity.Evidence = dto.Evidence;
        entity.EvidenceSource = dto.EvidenceSource;
        entity.CourtArgument = dto.CourtArgument;
        entity.Strength = dto.Strength;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Contradiction?> SetUsedAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Contradictions.FirstOrDefaultAsync(c => c.CaseId == caseId && c.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.Used = true;
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Contradictions.FirstOrDefaultAsync(c => c.CaseId == caseId && c.Id == id, cancellationToken);
        if (entity is null) return false;

        db.Contradictions.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ContradictionSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var items = await db.Contradictions.AsNoTracking().Where(c => c.CaseId == caseId).ToListAsync(cancellationToken);
        return new ContradictionSummary(items.Count, items.Count(c => c.Used), items.Count(c => !c.Used));
    }

    private static Contradiction Map(Guid caseId, CreateContradictionDto dto) => new()
    {
        CaseId = caseId,
        Claim = dto.Claim,
        ClaimSource = dto.ClaimSource,
        Evidence = dto.Evidence,
        EvidenceSource = dto.EvidenceSource,
        CourtArgument = dto.CourtArgument,
        Strength = dto.Strength,
    };
}
