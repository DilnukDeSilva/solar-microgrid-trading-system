/*
 * File: CancelReservationRequestDto.cs
 * Description: Optional body for POST /api/reservations/{id}/cancel.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Api.DTOs;

public class CancelReservationRequestDto
{
    public string? Reason { get; set; }
}
