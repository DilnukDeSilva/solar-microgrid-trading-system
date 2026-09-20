/*
 * File: HealthController.cs
 * Description: Anonymous health probe used to prove the API is reachable on LAN/IIS.
 * Author: Member 1
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    // Returns process health and current UTC server time.
    [HttpGet("/health")]
    [HttpGet("/api/health")]
    [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status200OK)]
    public ActionResult<HealthResponseDto> Get()
    {
        return Ok(new HealthResponseDto
        {
            Status = "Healthy",
            ServerTime = DateTime.UtcNow
        });
    }
}
