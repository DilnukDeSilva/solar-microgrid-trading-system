/*
 * File: OperatorController.cs
 * Description: Grid Operator QR verification and job completion. The client only displays the API result.
 * Author: Herath D M S T (IT22639776)
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Roles = RoleNames.GridOperator)]
[Produces("application/json")]
public class OperatorController : ControllerBase
{
    private readonly IOperatorService _operators;

    // Injects the service that checks and consumes a QR token.
    public OperatorController(IOperatorService operators)
    {
        _operators = operators;
    }

    // Looks up a QR token and returns the booking, or QR_INVALID / QR_ALREADY_USED.
    [HttpPost("verify-qr")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> VerifyQr([FromBody] VerifyQrRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _operators.VerifyQrAsync(request.QrToken, cancellationToken));
    }

    // Completes an approved booking after the same checks used by verify-qr.
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Complete(string id, CancellationToken cancellationToken)
    {
        var actor = new ReservationActor(
            User.FindFirst("sub")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty,
            User.FindFirst("nic")?.Value);

        return Ok(await _operators.CompleteAsync(id, actor, cancellationToken));
    }
}
