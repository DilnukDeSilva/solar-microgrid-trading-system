/*
 * File: UpdateReservationRequestDto.cs
 * Description: Body for PUT /api/reservations/{id}. StationId is only needed when moving to another station.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Api.DTOs;

public class UpdateReservationRequestDto
{
    public string? StationId { get; set; }

    public string SlotId { get; set; } = string.Empty;
}
