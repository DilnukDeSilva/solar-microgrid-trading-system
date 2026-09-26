/*
 * File: ReservationsController.cs
 * Description: Reservation endpoints for web and mobile. Only reads the request and calls ReservationService.
 * Author: Janukshan S (IT22635266)
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
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;

    // Injects the service that holds the reservation rules.
    public ReservationsController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    // Creates a booking. Staff send prosumerNic to book on behalf of a prosumer.
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _reservations.CreateAsync(request, CurrentActor(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Returns one booking. Prosumers get 403 for someone else's booking.
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetById(string id, CancellationToken cancellationToken)
    {
        return Ok(await _reservations.GetByIdAsync(id, CurrentActor(), cancellationToken));
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
