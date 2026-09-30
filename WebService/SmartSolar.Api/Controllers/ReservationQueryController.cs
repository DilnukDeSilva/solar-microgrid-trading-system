/*
 * File: ReservationQueryController.cs
 * Description: Reservation list, history filters and the operator approval queue.
 * Author: samudith
 * Created: 29/09/2026
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

    // Injects the read service that applies prosumer scoping.
    public ReservationQueryController(IReservationQueryService queries)
    {
        _queries = queries;
    }

    // Lists bookings. Filters combine. A prosumer's NIC comes from the token, not the query.
    [HttpGet]
    [ProducesResponseType(typeof(ReservationPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReservationPageDto>> Search(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? stationId,
        [FromQuery] string? q,
        [FromQuery] string? nic,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _queries.SearchAsync(CurrentActor(), status, from, to, stationId, q, nic, page, cancellationToken));
    }

    // Returns future Pending bookings for Backoffice and Grid Operators.
    [HttpGet("pending")]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    [ProducesResponseType(typeof(ReservationPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReservationPageDto>> Pending([FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        return Ok(await _queries.GetPendingQueueAsync(page, cancellationToken));
    }

    // Builds the current user from the JWT claims.
    private ReservationActor CurrentActor()
    {
        return new ReservationActor(
            User.FindFirst("sub")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty,
            User.FindFirst("nic")?.Value);
    }
}
