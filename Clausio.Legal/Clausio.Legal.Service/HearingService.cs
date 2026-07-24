using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IHearingService
{
    Task<List<Hearing>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Hearing> CreateAsync(Guid caseId, CreateHearingDto dto, CancellationToken cancellationToken = default);
    Task<HearingOrder?> SetOrderDoneAsync(Guid caseId, Guid hearingId, Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
}

public class HearingService(ClausioDbContext db) : IHearingService
{
    public Task<List<Hearing>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.Hearings.AsNoTracking().Include(h => h.Orders)
            .Where(h => h.CaseId == caseId)
            .OrderByDescending(h => h.HearingDate)
            .ToListAsync(cancellationToken);

    public async Task<Hearing> CreateAsync(Guid caseId, CreateHearingDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Hearing
        {
            CaseId = caseId,
            HearingDate = dto.HearingDate,
            Stage = dto.Stage,
            Judge = dto.Judge,
            CourtHall = dto.CourtHall,
            WhatHappened = dto.WhatHappened,
            JudgeObservation = dto.JudgeObservation,
            OpposingAdmission = dto.OpposingAdmission,
            NextObjective = dto.NextObjective,
        };

        if (dto.Orders is not null)
        {
            foreach (var order in dto.Orders)
            {
                entity.Orders.Add(new HearingOrder
                {
                    Text = order.Text,
                    Responsible = order.Responsible,
                    Deadline = order.Deadline,
                });
            }
        }

        db.Hearings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<HearingOrder?> SetOrderDoneAsync(Guid caseId, Guid hearingId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await db.HearingOrders
            .Include(o => o.Hearing)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.HearingId == hearingId && o.Hearing!.CaseId == caseId, cancellationToken);
        if (order is null) return null;

        order.Done = true;
        await db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Hearings.FirstOrDefaultAsync(h => h.CaseId == caseId && h.Id == id, cancellationToken);
        if (entity is null) return false;

        db.Hearings.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
