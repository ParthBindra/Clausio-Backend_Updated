using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface ITimelineService
{
    Task<List<TimelineEvent>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<TimelineEvent> CreateAsync(Guid caseId, CreateTimelineEventDto dto, CancellationToken cancellationToken = default);
    Task<List<TimelineEvent>> CreateBulkAsync(Guid caseId, List<CreateTimelineEventDto> dtos, CancellationToken cancellationToken = default);
    Task<TimelineEvent?> UpdateAsync(Guid caseId, Guid id, CreateTimelineEventDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
    Task ReorderAsync(Guid caseId, List<ReorderDto> items, CancellationToken cancellationToken = default);
}

public class TimelineService(ClausioDbContext db) : ITimelineService
{
    public Task<List<TimelineEvent>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.TimelineEvents.AsNoTracking().Where(t => t.CaseId == caseId).OrderBy(t => t.SortOrder).ThenBy(t => t.EventDate).ToListAsync(cancellationToken);

    public async Task<TimelineEvent> CreateAsync(Guid caseId, CreateTimelineEventDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Map(caseId, dto);
        db.TimelineEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<List<TimelineEvent>> CreateBulkAsync(Guid caseId, List<CreateTimelineEventDto> dtos, CancellationToken cancellationToken = default)
    {
        var entities = dtos.Select(dto => Map(caseId, dto)).ToList();
        db.TimelineEvents.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TimelineEvent?> UpdateAsync(Guid caseId, Guid id, CreateTimelineEventDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.TimelineEvents.FirstOrDefaultAsync(t => t.CaseId == caseId && t.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.EventDate = dto.EventDate;
        entity.Event = dto.Event;
        entity.Source = dto.Source;
        entity.LegalSignificance = dto.LegalSignificance;
        entity.Category = dto.Category;
        entity.SortOrder = dto.SortOrder;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.TimelineEvents.FirstOrDefaultAsync(t => t.CaseId == caseId && t.Id == id, cancellationToken);
        if (entity is null) return false;

        db.TimelineEvents.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReorderAsync(Guid caseId, List<ReorderDto> items, CancellationToken cancellationToken = default)
    {
        var ids = items.Select(i => i.Id).ToList();
        var entities = await db.TimelineEvents.Where(t => t.CaseId == caseId && ids.Contains(t.Id)).ToListAsync(cancellationToken);
        var lookup = items.ToDictionary(i => i.Id, i => i.SortOrder);

        foreach (var entity in entities)
        {
            entity.SortOrder = lookup[entity.Id];
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static TimelineEvent Map(Guid caseId, CreateTimelineEventDto dto) => new()
    {
        CaseId = caseId,
        EventDate = dto.EventDate,
        Event = dto.Event,
        Source = dto.Source,
        LegalSignificance = dto.LegalSignificance,
        Category = dto.Category,
        SortOrder = dto.SortOrder,
    };
}
