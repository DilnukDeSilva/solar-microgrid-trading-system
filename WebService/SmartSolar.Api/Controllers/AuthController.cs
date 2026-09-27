/*
 * File: AuthController.cs
 * Description: Thin HTTP adapter for login. All credential rules live in AuthService.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Injects the auth use-case used by POST /api/auth/login.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Issues a JWT when username/NIC and password match an Active account.
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    // Creates a Pending prosumer; no JWT is issued until Backoffice activates the account.
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterProsumerRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _authService.RegisterAsync(request, cancellationToken);
        return Created($"/api/prosumers/{Uri.EscapeDataString(created.Nic!)}", created);
    }
}
