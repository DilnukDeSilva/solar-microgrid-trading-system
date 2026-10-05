/*
 * File: IReservationQueryService.cs
 * Description: List, history and dashboard reads. Prosumer scope is applied here from the JWT.
 * Author: Herath D M S T (IT22639776)
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IReservationQueryService
{
    // Lists reservations. A prosumer only ever sees their own NIC, taken from the token.
    Task<ReservationPageDto> SearchAsync(
        ReservationActor actor,
        string? status,
        DateTime? from,
        DateTime? to,
        string? stationId,
        string? query,
        string? nic,
        int page,
        CancellationToken cancellationToken = default);

    // Returns the operator approval queue: future Pending bookings.
    Task<ReservationPageDto> GetPendingQueueAsync(int page, CancellationToken cancellationToken = default);

    // Returns the signed-in prosumer's live dashboard counts and next booking.
    Task<ProsumerDashboardDto> GetProsumerDashboardAsync(ReservationActor actor, CancellationToken cancellationToken = default);

    // Returns today's operational counts for the staff Home page.
    Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default);
}
