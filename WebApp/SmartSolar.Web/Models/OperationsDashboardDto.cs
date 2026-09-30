/*
 * File: OperationsDashboardDto.cs
 * Description: Staff Home figures from GET /dashboard/operations.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Web.Models;

public class OperationsDashboardDto
{
    public int TodaysBookingCount { get; set; }
    public int PendingApprovals { get; set; }
    public int ActiveStations { get; set; }
    public int CompletedToday { get; set; }
    public List<ReservationDto> TodaysBookings { get; set; } = new();
}
