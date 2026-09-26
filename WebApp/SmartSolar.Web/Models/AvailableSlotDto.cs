/*
 * File: AvailableSlotDto.cs
 * Description: A free slot returned by the API for the booking picker.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

public class AvailableSlotDto
{
    public string Id { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
