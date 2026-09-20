/*
 * File: ReservationStatuses.cs
 * Description: Allowed values for EnergyReservation.status.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Common;

public static class ReservationStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";
}
