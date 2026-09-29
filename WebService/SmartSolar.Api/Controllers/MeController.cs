/*
 * File: MeController.cs
 * Description: Current prosumer profile endpoints; identity always comes from the JWT.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Roles = RoleNames.Prosumer)]
[Produces("application/json")]
public class MeController : ControllerBase
{
    private readonly IProsumerService _prosumers;

    public MeController(IProsumerService prosumers)
    {
        _prosumers = prosumers;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.GetMeAsync(CurrentNic(), cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> Update([FromBody] UpdateProsumerRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.UpdateMeAsync(CurrentNic(), request, cancellationToken));
    }

    [HttpPost("request-deactivation")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> RequestDeactivation(CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.DeactivateAsync(CurrentNic(), cancellationToken));
    }

    private string CurrentNic()
    {
        return User.FindFirst("nic")?.Value
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, "The token does not contain a prosumer NIC.");
    }
}
