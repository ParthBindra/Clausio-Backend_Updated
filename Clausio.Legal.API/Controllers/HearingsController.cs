using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/hearings")]
public class HearingsController(IHearingService hearingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await hearingService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid caseId, CreateHearingDto dto, CancellationToken cancellationToken) =>
        Ok(await hearingService.CreateAsync(caseId, dto, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid caseId, Guid id, UpdateHearingDto dto, CancellationToken cancellationToken)
    {
        var hearing = await hearingService.UpdateAsync(caseId, id, dto, cancellationToken);
        return hearing is null ? NotFound() : Ok(hearing);
    }

    [HttpPut("{id:guid}/orders/{orderId:guid}/done")]
    public async Task<IActionResult> MarkOrderDone(Guid caseId, Guid id, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await hearingService.SetOrderDoneAsync(caseId, id, orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await hearingService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();
}
