/*
 * File: Slot.cs
 * Description: MongoDB document for EnergyBookingSlots. Times are stored as UTC.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.Api.Models;

public class Slot
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    public string StationId { get; set; } = string.Empty;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartTime { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; } = true;
}
