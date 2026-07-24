using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/timeline")]
public class TimelineController(ITimelineService timelineService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await timelineService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid caseId, CreateTimelineEventDto dto, CancellationToken cancellationToken) =>
        Ok(await timelineService.CreateAsync(caseId, dto, cancellationToken));

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk(Guid caseId, List<CreateTimelineEventDto> dtos, CancellationToken cancellationToken) =>
        Ok(await timelineService.CreateBulkAsync(caseId, dtos, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid caseId, Guid id, CreateTimelineEventDto dto, CancellationToken cancellationToken)
    {
        var entity = await timelineService.UpdateAsync(caseId, id, dto, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await timelineService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(Guid caseId, List<ReorderDto> items, CancellationToken cancellationToken)
    {
        await timelineService.ReorderAsync(caseId, items, cancellationToken);
        return Ok();
    }
}
