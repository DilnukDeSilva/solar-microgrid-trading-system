/*
 * File: AvailableSlotDto.cs
 * Description: A free slot that can be booked right now (inside the 7-day window).
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.DTOs;

public class AvailableSlotDto
{
    public string Id { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Maps a slot document to the picker shape.
    public static AvailableSlotDto From(Slot slot)
    {
        return new AvailableSlotDto
        {
            Id = slot.Id,
            StationId = slot.StationId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        };
    }
}
