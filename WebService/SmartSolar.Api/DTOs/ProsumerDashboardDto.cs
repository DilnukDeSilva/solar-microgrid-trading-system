/*
 * File: ProsumerDashboardDto.cs
 * Description: Live counts and the next booking for GET /api/dashboard/me.
 * Author: Herath D M S T (IT22639776)
 */

namespace SmartSolar.Api.DTOs;

public class ProsumerDashboardDto
{
    public int PendingCount { get; set; }
    public int ApprovedFutureCount { get; set; }
    public int ActiveCount { get; set; }
    public ReservationDto? NextReservation { get; set; }
}
