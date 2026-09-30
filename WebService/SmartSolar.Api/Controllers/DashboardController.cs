/*
 * File: DashboardController.cs
 * Description: Prosumer and operations dashboards. Every count is read from the database.
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
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IReservationQueryService _queries;

    // Injects the read service that computes the dashboard figures.
    public DashboardController(IReservationQueryService queries)
    {
        _queries = queries;
    }

    // Returns the signed-in prosumer's pending, approved-future and next booking.
    [HttpGet("me")]
    [Authorize(Roles = RoleNames.Prosumer)]
    [ProducesResponseType(typeof(ProsumerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProsumerDashboardDto>> Me(CancellationToken cancellationToken)
    {
        var actor = new ReservationActor(
            User.FindFirst("sub")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty,
            User.FindFirst("nic")?.Value);

        return Ok(await _queries.GetProsumerDashboardAsync(actor, cancellationToken));
    }

    // Returns today's bookings, pending approvals, active stations and completions for the web Home.
    [HttpGet("operations")]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    [ProducesResponseType(typeof(OperationsDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperationsDashboardDto>> Operations(CancellationToken cancellationToken)
    {
        return Ok(await _queries.GetOperationsDashboardAsync(cancellationToken));
    }
}
