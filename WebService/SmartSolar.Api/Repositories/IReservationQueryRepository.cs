/*
 * File: IReservationQueryRepository.cs
 * Description: Read-only reservation queries for lists, dashboards and QR lookup.
 * Author: Herath D M S T (IT22639776)
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public interface IReservationQueryRepository
{
    // Returns one page of reservations matching every supplied filter, sorted by scheduledAt.
    Task<(IReadOnlyList<Reservation> Items, long TotalCount)> SearchAsync(
        string? prosumerNic,
        string? status,
        string? stationId,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? query,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    // Counts reservations for one prosumer, optionally by status and a scheduledAt lower bound.
    Task<long> CountForProsumerAsync(
        string prosumerNic,
        string? status,
        DateTime? scheduledAfterUtc,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    // Returns the soonest future Pending or Approved reservation for one prosumer.
    Task<Reservation?> FindNextUpcomingAsync(string prosumerNic, DateTime nowUtc, CancellationToken cancellationToken = default);

    // Counts reservations whose scheduledAt falls in [startUtc, endUtc).
    Task<long> CountScheduledBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    // Counts Pending reservations that have not started yet.
    Task<long> CountPendingFutureAsync(DateTime nowUtc, CancellationToken cancellationToken = default);

    // Counts reservations completed during [startUtc, endUtc).
    Task<long> CountCompletedBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    // Lists reservations scheduled during [startUtc, endUtc), earliest first.
    Task<IReadOnlyList<Reservation>> ListScheduledBetweenAsync(
        DateTime startUtc,
        DateTime endUtc,
        int limit,
        CancellationToken cancellationToken = default);

    // Loads the reservation that carries this QR token, including one already completed.
    Task<Reservation?> FindByQrTokenAsync(string qrToken, CancellationToken cancellationToken = default);
}
