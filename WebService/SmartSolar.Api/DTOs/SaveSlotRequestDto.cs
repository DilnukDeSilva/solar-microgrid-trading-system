/*
 * File: SaveSlotRequestDto.cs
 * Description: Body for creating or updating a slot. Times are UTC.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class SaveSlotRequestDto
{
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}
