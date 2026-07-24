using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await clientService.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateClientDto dto, CancellationToken cancellationToken) =>
        Ok(await clientService.CreateAsync(dto, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var client = await clientService.GetAsync(id, cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateClientDto dto, CancellationToken cancellationToken)
    {
        var client = await clientService.UpdateAsync(id, dto, cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await clientService.DeleteAsync(id, cancellationToken) ? Ok() : NotFound();
}
