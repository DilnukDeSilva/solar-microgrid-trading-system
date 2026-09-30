/*
 * File: Station.cs
 * Description: MongoDB document for SolarStationInfo, including weekly opening hours.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.Api.Models;

public class Station
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double CapacityKwh { get; set; }

    public int BatterySlotsTotal { get; set; }

    public List<StationScheduleEntry> Schedule { get; set; } = new();

    public string Status { get; set; } = string.Empty;
}

public class StationScheduleEntry
{
    public string DayOfWeek { get; set; } = string.Empty;

    public string OpenTime { get; set; } = string.Empty;

    public string CloseTime { get; set; } = string.Empty;
}
