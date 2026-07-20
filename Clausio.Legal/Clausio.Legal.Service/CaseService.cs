using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface ICaseService
{
    Task<List<Case>> ListAsync(CancellationToken cancellationToken = default);
    Task<Case?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Case> CreateAsync(CreateCaseDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Case?> UpdateAsync(Guid id, UpdateCaseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CaseService(ClausioDbContext db) : ICaseService
{
    public Task<List<Case>> ListAsync(CancellationToken cancellationToken = default) =>
        db.Cases.AsNoTracking().Include(c => c.Client).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

    public Task<Case?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Cases.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Case> CreateAsync(CreateCaseDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var entity = new Case
        {
            Name = dto.Name,
            CaseNumber = dto.CaseNumber,
            CaseType = dto.CaseType,
            SubType = dto.SubType,
            Court = dto.Court,
            CourtLocation = dto.CourtLocation,
            Stage = dto.Stage,
            Status = "Active",
            Priority = dto.Priority,
            OpposingAdv = dto.OpposingAdv,
            FiledOn = dto.FiledOn,
            NextHearing = dto.NextHearing,
            ClientId = dto.ClientId,
            CreatedByUserId = createdByUserId,
        };

        db.Cases.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Case?> UpdateAsync(Guid id, UpdateCaseDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Cases.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Stage is not null) entity.Stage = dto.Stage;
        if (dto.Status is not null) entity.Status = dto.Status;
        if (dto.Priority is not null) entity.Priority = dto.Priority;
        if (dto.OpposingAdv is not null) entity.OpposingAdv = dto.OpposingAdv;
        if (dto.NextHearing is not null) entity.NextHearing = dto.NextHearing;
        if (dto.ReadinessScore is not null) entity.ReadinessScore = dto.ReadinessScore;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Cases.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null) return false;

        db.Cases.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
