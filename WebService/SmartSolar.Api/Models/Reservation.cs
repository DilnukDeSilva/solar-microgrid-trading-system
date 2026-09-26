/*
 * File: Reservation.cs
 * Description: MongoDB document for EnergyReservation, including the operator QR token.
 * Author: Member 1
 * Created: 20/09/2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.Api.Models;

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    public string ProsumerNic { get; set; } = string.Empty;

    public string StationId { get; set; } = string.Empty;

    public string StationName { get; set; } = string.Empty;

    public string SlotId { get; set; } = string.Empty;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ScheduledAt { get; set; }

    public string Status { get; set; } = string.Empty;

    [BsonIgnoreIfNull]
    public string? QrToken { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonIgnoreIfNull]
    public string? CreatedBy { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CompletedAt { get; set; }

    [BsonIgnoreIfNull]
    public string? CompletedBy { get; set; }

    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ApprovedAt { get; set; }

    [BsonIgnoreIfNull]
    public string? ApprovedBy { get; set; }

    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CancelledAt { get; set; }

    [BsonIgnoreIfNull]
    public string? CancelledBy { get; set; }

    [BsonIgnoreIfNull]
    public string? CancelReason { get; set; }
}
