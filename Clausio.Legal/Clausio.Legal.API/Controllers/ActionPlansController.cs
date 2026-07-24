using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/actionplans")]
public class ActionPlansController(IActionPlanService actionPlanService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await actionPlanService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid caseId, CreateActionPlanDto dto, CancellationToken cancellationToken) =>
        Ok(await actionPlanService.CreateAsync(caseId, dto, cancellationToken));

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk(Guid caseId, List<CreateActionPlanDto> dtos, CancellationToken cancellationToken) =>
        Ok(await actionPlanService.CreateBulkAsync(caseId, dtos, cancellationToken));

    [HttpPut("{id:guid}/done")]
    public async Task<IActionResult> MarkDone(Guid caseId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await actionPlanService.SetDoneAsync(caseId, id, true, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("{id:guid}/undone")]
    public async Task<IActionResult> MarkUndone(Guid caseId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await actionPlanService.SetDoneAsync(caseId, id, false, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid caseId, Guid id, CreateActionPlanDto dto, CancellationToken cancellationToken)
    {
        var entity = await actionPlanService.UpdateAsync(caseId, id, dto, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await actionPlanService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await actionPlanService.GetSummaryAsync(caseId, cancellationToken));
}
