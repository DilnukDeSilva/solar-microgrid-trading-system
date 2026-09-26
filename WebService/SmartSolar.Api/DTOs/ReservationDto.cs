/*
 * File: ReservationDto.cs
 * Description: Reservation shape returned to both clients, as defined in the API contract.
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.DTOs;

public class ReservationDto
{
    public string Id { get; set; } = string.Empty;
    public string ProsumerNic { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string SlotId { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? QrToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }

    // Maps a reservation document to the contract shape.
    public static ReservationDto From(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            ProsumerNic = reservation.ProsumerNic,
            StationId = reservation.StationId,
            StationName = reservation.StationName,
            SlotId = reservation.SlotId,
            ScheduledAt = reservation.ScheduledAt,
            Status = reservation.Status,
            QrToken = reservation.QrToken,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt,
            CompletedAt = reservation.CompletedAt,
            CompletedBy = reservation.CompletedBy
        };
    }
}
