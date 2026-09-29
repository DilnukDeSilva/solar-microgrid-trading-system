/*
 * File: OperationsDashboardDto.cs
 * Description: Live counts for the staff Home page from GET /api/dashboard/operations.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class OperationsDashboardDto
{
    public int TodaysBookingCount { get; set; }
    public int PendingApprovals { get; set; }
    public int ActiveStations { get; set; }
    public int CompletedToday { get; set; }
    public IReadOnlyList<ReservationDto> TodaysBookings { get; set; } = Array.Empty<ReservationDto>();
}
