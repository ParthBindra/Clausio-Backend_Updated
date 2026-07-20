using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/research")]
public class LegalResearchController(ILegalResearchService legalResearchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await legalResearchService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid caseId, CreateLegalResearchDto dto, CancellationToken cancellationToken) =>
        Ok(await legalResearchService.CreateAsync(caseId, dto, cancellationToken));

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk(Guid caseId, List<CreateLegalResearchDto> dtos, CancellationToken cancellationToken) =>
        Ok(await legalResearchService.CreateBulkAsync(caseId, dtos, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid caseId, Guid id, CreateLegalResearchDto dto, CancellationToken cancellationToken)
    {
        var entity = await legalResearchService.UpdateAsync(caseId, id, dto, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await legalResearchService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await legalResearchService.GetSummaryAsync(caseId, cancellationToken));
}
