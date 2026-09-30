/*
 * File: NearbyStationDto.cs
 * Description: One Active station inside a search radius, with distance and free slots.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class NearbyStationDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double CapacityKwh { get; set; }

    public int BatterySlotsTotal { get; set; }

    public double DistanceKm { get; set; }

    public int FreeSlots { get; set; }
}
