/*
 * File: StationRepository.cs
 * Description: MongoDB access for SolarStationInfo.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class StationRepository : IStationRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by station queries.
    public StationRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns every station document.
    public async Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stations.Find(FilterDefinition<Station>.Empty).ToListAsync(cancellationToken);
    }

    // Loads one station by id.
    public async Task<Station?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Stations.Find(station => station.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    // Inserts a new station document.
    public Task InsertAsync(Station station, CancellationToken cancellationToken = default)
    {
        return _context.Stations.InsertOneAsync(station, cancellationToken: cancellationToken);
    }

    // Replaces an existing station document by id.
    public Task ReplaceAsync(Station station, CancellationToken cancellationToken = default)
    {
        return _context.Stations.ReplaceOneAsync(existing => existing.Id == station.Id, station, cancellationToken: cancellationToken);
    }

    // True when a future Pending or Approved reservation still points at this station.
    public Task<bool> HasActiveFutureReservationsAsync(string stationId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Eq(reservation => reservation.StationId, stationId)
            & Builders<Reservation>.Filter.Gt(reservation => reservation.ScheduledAt, nowUtc)
            & Builders<Reservation>.Filter.In(
                reservation => reservation.Status,
                new[] { ReservationStatuses.Pending, ReservationStatuses.Approved });

        return _context.Reservations.Find(filter).AnyAsync(cancellationToken);
    }
}
