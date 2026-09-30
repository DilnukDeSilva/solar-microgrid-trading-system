/*
 * File: SlotDto.cs
 * Description: Slot JSON returned by the API. Times are UTC.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

namespace SmartSolar.Web.Models;

public class SlotDto
{
    public string Id { get; set; } = string.Empty;

    public string StationId { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
}
