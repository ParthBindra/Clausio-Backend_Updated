using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cases/{caseId:guid}/documents")]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid caseId, CancellationToken cancellationToken) =>
        Ok(await documentService.ListAsync(caseId, cancellationToken));

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Upload(
        Guid caseId,
        IFormFile file,
        [FromForm] string? documentType,
        [FromForm] string? exhibitLabel,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("A non-empty file is required.");
        }

        await using var stream = file.OpenReadStream();
        var document = await documentService.UploadAsync(
            caseId, file.FileName, file.ContentType, documentType, exhibitLabel, stream, file.Length, cancellationToken);
        return Ok(document);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid caseId, Guid id, CancellationToken cancellationToken) =>
        await documentService.DeleteAsync(caseId, id, cancellationToken) ? Ok() : NotFound();
}
