/*
 * File: ReservationDto.cs
 * Description: Reservation JSON shape returned by the API.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

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
}
