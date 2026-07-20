using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/contradictions")]
public class ContradictionsController(IContradictionService contradictionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await contradictionService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid caseId, CreateContradictionDto dto, CancellationToken cancellationToken) =>
        Ok(await contradictionService.CreateAsync(caseId, dto, cancellationToken));

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk(Guid caseId, List<CreateContradictionDto> dtos, CancellationToken cancellationToken) =>
        Ok(await contradictionService.CreateBulkAsync(caseId, dtos, cancellationToken));

    [HttpPut("{id:guid}/used")]
    public async Task<IActionResult> MarkUsed(Guid caseId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await contradictionService.SetUsedAsync(caseId, id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid caseId, Guid id, CreateContradictionDto dto, CancellationToken cancellationToken)
    {
        var entity = await contradictionService.UpdateAsync(caseId, id, dto, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await contradictionService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await contradictionService.GetSummaryAsync(caseId, cancellationToken));
}
