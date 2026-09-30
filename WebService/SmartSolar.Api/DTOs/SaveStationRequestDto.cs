/*
 * File: SaveStationRequestDto.cs
 * Description: Body for creating or updating a station. Validation lives in StationService.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.DTOs;

public class SaveStationRequestDto
{
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double CapacityKwh { get; set; }

    public int BatterySlotsTotal { get; set; }

    public List<StationScheduleEntry> Schedule { get; set; } = new();
}
