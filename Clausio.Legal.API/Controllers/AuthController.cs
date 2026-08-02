using Clausio.Legal.API.Extensions;
using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clausio.Legal.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.RegisterAsync(dto, cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken) =>
        Ok(await authService.LoginAsync(dto, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await authService.GetCurrentUserAsync(User.GetUserId(), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await authService.ChangePasswordAsync(userId, dto, cancellationToken);
        return Ok(new { message = "Password changed successfully." });
    }
}
