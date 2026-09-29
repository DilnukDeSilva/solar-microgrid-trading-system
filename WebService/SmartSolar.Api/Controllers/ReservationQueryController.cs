/*
 * File: ReservationQueryController.cs
 * Description: GET /api/reservations list endpoint. Owned by Member 4; stand-in written by Janukshan S (IT22635266) so the web list works before merge.
 * Author: Member 4
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Roles = RoleNames.Prosumer + "," + RoleNames.Backoffice + "," + RoleNames.GridOperator)]
[Produces("application/json")]
public class ReservationQueryController : ControllerBase
{
    private readonly IReservationQueryService _queries;

    // Injects the reservation query service.
    public ReservationQueryController(IReservationQueryService queries)
    {
        _queries = queries;
    }

    // Lists reservations. Filters: status, from/to (slot time), q (station, NIC or reference), nic (staff only).
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> Search(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? q,
        [FromQuery] string? nic,
        CancellationToken cancellationToken)
    {
        var actor = new ReservationActor(
            User.FindFirst("sub")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty,
            User.FindFirst("nic")?.Value);

        return Ok(await _queries.SearchAsync(status, from, to, q, nic, actor, cancellationToken));
    }
}
