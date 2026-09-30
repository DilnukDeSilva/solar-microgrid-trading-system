/*
 * File: SlotsController.cs
 * Description: HTTP endpoints for listing, creating, updating, deleting and opening or closing slots. Rules stay in SlotService.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Produces("application/json")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slots;

    // Injects the slot service that owns the schedule and battery rules.
    public SlotsController(ISlotService slots)
    {
        _slots = slots;
    }

    // Lists a station's slots for any logged-in role.
    [HttpGet("api/stations/{stationId}/slots")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<Slot>>> List(string stationId, CancellationToken cancellationToken)
    {
        return Ok(await _slots.ListByStationAsync(stationId, cancellationToken));
    }

    // Creates a slot. Backoffice and GridOperator.
    [HttpPost("api/stations/{stationId}/slots")]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    public async Task<ActionResult<Slot>> Create(
        string stationId,
        [FromBody] SaveSlotRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _slots.CreateAsync(stationId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { stationId = created.StationId }, created);
    }

    // Updates a slot. Backoffice and GridOperator.
    [HttpPut("api/slots/{id}")]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    public async Task<ActionResult<Slot>> Update(
        string id,
        [FromBody] SaveSlotRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _slots.UpdateAsync(id, request, cancellationToken));
    }

    // Deletes a slot. Backoffice and GridOperator.
    [HttpDelete("api/slots/{id}")]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _slots.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // Opens or closes a free slot. GridOperator only.
    [HttpPut("api/slots/{id}/availability")]
    [Authorize(Roles = RoleNames.GridOperator)]
    public async Task<ActionResult<Slot>> SetAvailability(
        string id,
        [FromBody] SlotAvailabilityRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _slots.SetAvailabilityAsync(id, request, cancellationToken));
    }
}
