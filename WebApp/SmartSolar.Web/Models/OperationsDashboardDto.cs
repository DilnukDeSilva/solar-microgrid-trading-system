/*
 * File: OperationsDashboardDto.cs
 * Description: Staff Home figures from GET /dashboard/operations.
 * Author: Herath D M S T (IT22639776)
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
