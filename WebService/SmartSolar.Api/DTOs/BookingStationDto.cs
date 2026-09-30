/*
 * File: BookingStationDto.cs
 * Description: A station that is currently taking bookings, used by the booking pickers.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Api.DTOs;

public class BookingStationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
