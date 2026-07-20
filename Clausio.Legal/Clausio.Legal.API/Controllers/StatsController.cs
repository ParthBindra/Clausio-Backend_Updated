using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/stats")]
public class StatsController(IStatsService statsService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken) =>
        Ok(await statsService.GetOverviewAsync(cancellationToken));

    [HttpGet("cases")]
    public async Task<IActionResult> Cases(CancellationToken cancellationToken) =>
        Ok(await statsService.GetCaseStatsAsync(cancellationToken));

    [HttpGet("hearings")]
    public async Task<IActionResult> Hearings(CancellationToken cancellationToken) =>
        Ok(await statsService.GetHearingStatsAsync(cancellationToken));

    [HttpGet("documents")]
    public async Task<IActionResult> Documents(CancellationToken cancellationToken) =>
        Ok(await statsService.GetDocumentStatsAsync(cancellationToken));

    [HttpGet("activity")]
    public async Task<IActionResult> Activity(CancellationToken cancellationToken) =>
        Ok(await statsService.GetActivityAsync(cancellationToken));
}
