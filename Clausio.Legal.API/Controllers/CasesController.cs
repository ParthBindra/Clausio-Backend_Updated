using Clausio.Legal.API.Extensions;
using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases")]
public class CasesController(ICaseService caseService) : ControllerBase
{
    // ✅ FIXED — passes userId so only this user's cases are returned
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await caseService.ListAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCaseDto dto, CancellationToken cancellationToken) =>
        Ok(await caseService.CreateAsync(dto, User.GetUserId(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var caseEntity = await caseService.GetAsync(id, cancellationToken);
        return caseEntity is null ? NotFound() : Ok(caseEntity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCaseDto dto, CancellationToken cancellationToken)
    {
        var caseEntity = await caseService.UpdateAsync(id, dto, cancellationToken);
        return caseEntity is null ? NotFound() : Ok(caseEntity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await caseService.DeleteAsync(id, cancellationToken) ? Ok() : NotFound();
}
