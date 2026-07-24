using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public record ActionPlanSummary(int Total, int Done, int Pending, int Overdue);

public interface IActionPlanService
{
    Task<List<ActionPlan>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<ActionPlan> CreateAsync(Guid caseId, CreateActionPlanDto dto, CancellationToken cancellationToken = default);
    Task<List<ActionPlan>> CreateBulkAsync(Guid caseId, List<CreateActionPlanDto> dtos, CancellationToken cancellationToken = default);
    Task<ActionPlan?> UpdateAsync(Guid caseId, Guid id, CreateActionPlanDto dto, CancellationToken cancellationToken = default);
    Task<ActionPlan?> SetDoneAsync(Guid caseId, Guid id, bool done, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
    Task<ActionPlanSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default);
}

public class ActionPlanService(ClausioDbContext db) : IActionPlanService
{
    public Task<List<ActionPlan>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.ActionPlans.AsNoTracking().Where(a => a.CaseId == caseId).OrderBy(a => a.DueBy).ToListAsync(cancellationToken);

    public async Task<ActionPlan> CreateAsync(Guid caseId, CreateActionPlanDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Map(caseId, dto);
        db.ActionPlans.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<List<ActionPlan>> CreateBulkAsync(Guid caseId, List<CreateActionPlanDto> dtos, CancellationToken cancellationToken = default)
    {
        var entities = dtos.Select(dto => Map(caseId, dto)).ToList();
        db.ActionPlans.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<ActionPlan?> UpdateAsync(Guid caseId, Guid id, CreateActionPlanDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.ActionPlans.FirstOrDefaultAsync(a => a.CaseId == caseId && a.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Priority = dto.Priority;
        entity.DueBy = dto.DueBy;
        entity.AssignedTo = dto.AssignedTo;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ActionPlan?> SetDoneAsync(Guid caseId, Guid id, bool done, CancellationToken cancellationToken = default)
    {
        var entity = await db.ActionPlans.FirstOrDefaultAsync(a => a.CaseId == caseId && a.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.Done = done;
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.ActionPlans.FirstOrDefaultAsync(a => a.CaseId == caseId && a.Id == id, cancellationToken);
        if (entity is null) return false;

        db.ActionPlans.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ActionPlanSummary> GetSummaryAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var items = await db.ActionPlans.AsNoTracking().Where(a => a.CaseId == caseId).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        return new ActionPlanSummary(
            Total: items.Count,
            Done: items.Count(a => a.Done),
            Pending: items.Count(a => !a.Done),
            Overdue: items.Count(a => !a.Done && a.DueBy < now));
    }

    private static ActionPlan Map(Guid caseId, CreateActionPlanDto dto) => new()
    {
        CaseId = caseId,
        Title = dto.Title,
        Description = dto.Description,
        Priority = dto.Priority,
        DueBy = dto.DueBy,
        AssignedTo = dto.AssignedTo,
    };
}
