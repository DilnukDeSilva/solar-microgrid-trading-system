/*
 * File: ReservationQueryRepository.cs
 * Description: MongoDB queries for reservation lists, dashboard counts and QR lookup.
 * Author: Herath D M S T (IT22639776)
 */

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class ReservationQueryRepository : IReservationQueryRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by read queries.
    public ReservationQueryRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns one page of reservations matching every supplied filter, sorted by scheduledAt.
    public async Task<(IReadOnlyList<Reservation> Items, long TotalCount)> SearchAsync(
        string? prosumerNic,
        string? status,
        string? stationId,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? query,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(prosumerNic, status, stationId, fromUtc, toUtc, query, activeOnly: false, scheduledAfterUtc: null);
        var total = await _context.Reservations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _context.Reservations
            .Find(filter)
            .SortBy(reservation => reservation.ScheduledAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    // Counts reservations for one prosumer, optionally by status and a scheduledAt lower bound.
    public Task<long> CountForProsumerAsync(
        string prosumerNic,
        string? status,
        DateTime? scheduledAfterUtc,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(prosumerNic, status, null, null, null, null, activeOnly, scheduledAfterUtc);
        return _context.Reservations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    // Returns the soonest future Pending or Approved reservation for one prosumer.
    public Task<Reservation?> FindNextUpcomingAsync(string prosumerNic, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(prosumerNic, null, null, null, null, null, activeOnly: true, scheduledAfterUtc: nowUtc);
        return _context.Reservations
            .Find(filter)
            .SortBy(reservation => reservation.ScheduledAt)
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    // Counts reservations whose scheduledAt falls in [startUtc, endUtc).
    public Task<long> CountScheduledBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Gte(reservation => reservation.ScheduledAt, startUtc)
            & Builders<Reservation>.Filter.Lt(reservation => reservation.ScheduledAt, endUtc);
        return _context.Reservations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    // Counts Pending reservations that have not started yet.
    public Task<long> CountPendingFutureAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Eq(reservation => reservation.Status, ReservationStatuses.Pending)
            & Builders<Reservation>.Filter.Gt(reservation => reservation.ScheduledAt, nowUtc);
        return _context.Reservations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    // Counts reservations completed during [startUtc, endUtc).
    public Task<long> CountCompletedBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Eq(reservation => reservation.Status, ReservationStatuses.Completed)
            & Builders<Reservation>.Filter.Gte(reservation => reservation.CompletedAt, startUtc)
            & Builders<Reservation>.Filter.Lt(reservation => reservation.CompletedAt, endUtc);
        return _context.Reservations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    // Lists reservations scheduled during [startUtc, endUtc), earliest first.
    public async Task<IReadOnlyList<Reservation>> ListScheduledBetweenAsync(
        DateTime startUtc,
        DateTime endUtc,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Gte(reservation => reservation.ScheduledAt, startUtc)
            & Builders<Reservation>.Filter.Lt(reservation => reservation.ScheduledAt, endUtc);
        return await _context.Reservations
            .Find(filter)
            .SortBy(reservation => reservation.ScheduledAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    // Loads the reservation that carries this QR token, including one already completed.
    public Task<Reservation?> FindByQrTokenAsync(string qrToken, CancellationToken cancellationToken = default)
    {
        return _context.Reservations
            .Find(reservation => reservation.QrToken == qrToken)
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    // Combines equality, range and keyword filters. A blank value is ignored.
    private static FilterDefinition<Reservation> BuildFilter(
        string? prosumerNic,
        string? status,
        string? stationId,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? query,
        bool activeOnly,
        DateTime? scheduledAfterUtc)
    {
        var filter = Builders<Reservation>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(prosumerNic))
        {
            filter &= Builders<Reservation>.Filter.Eq(reservation => reservation.ProsumerNic, prosumerNic.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= Builders<Reservation>.Filter.Eq(reservation => reservation.Status, status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stationId))
        {
            filter &= Builders<Reservation>.Filter.Eq(reservation => reservation.StationId, stationId.Trim());
        }

        if (fromUtc is not null)
        {
            filter &= Builders<Reservation>.Filter.Gte(reservation => reservation.ScheduledAt, fromUtc.Value);
        }

        if (toUtc is not null)
        {
            filter &= Builders<Reservation>.Filter.Lte(reservation => reservation.ScheduledAt, toUtc.Value);
        }

        if (scheduledAfterUtc is not null)
        {
            filter &= Builders<Reservation>.Filter.Gt(reservation => reservation.ScheduledAt, scheduledAfterUtc.Value);
        }

        if (activeOnly)
        {
            filter &= Builders<Reservation>.Filter.In(
                reservation => reservation.Status,
                new[] { ReservationStatuses.Pending, ReservationStatuses.Approved });
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = new BsonRegularExpression(Regex.Escape(query.Trim()), "i");
            filter &= Builders<Reservation>.Filter.Or(
                Builders<Reservation>.Filter.Regex(reservation => reservation.StationName, pattern),
                Builders<Reservation>.Filter.Regex(reservation => reservation.ProsumerNic, pattern),
                Builders<Reservation>.Filter.Regex(reservation => reservation.Id, pattern));
        }

        return filter;
    }
}
