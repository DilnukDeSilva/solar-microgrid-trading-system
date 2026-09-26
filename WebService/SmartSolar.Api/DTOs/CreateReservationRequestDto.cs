/*
 * File: CreateReservationRequestDto.cs
 * Description: Body for POST /api/reservations. ProsumerNic is only read for staff bookings.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Api.DTOs;

public class CreateReservationRequestDto
{
    public string StationId { get; set; } = string.Empty;

    public string SlotId { get; set; } = string.Empty;

    public string? ProsumerNic { get; set; }
}
