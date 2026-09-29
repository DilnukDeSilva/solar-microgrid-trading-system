/*
 * File: ReservationRepository.cs
 * Description: MongoDB access for EnergyReservation.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by reservation queries.
    public ReservationRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns every reservation document.
    public async Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.Find(FilterDefinition<Reservation>.Empty).ToListAsync(cancellationToken);
    }

    // Loads one reservation by id.
    public async Task<Reservation?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.Find(reservation => reservation.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    // Finds reservations matching the optional filters, ordered by slot time.
    public async Task<IReadOnlyList<Reservation>> SearchAsync(string? nic, string? status, DateTime? from, DateTime? to, string? text, CancellationToken cancellationToken = default)
    {
        var f = Builders<Reservation>.Filter;
        var filter = f.Empty;

        if (!string.IsNullOrWhiteSpace(nic))
        {
            filter &= f.Eq(r => r.ProsumerNic, nic);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= f.Eq(r => r.Status, status);
        }

        if (from is not null)
        {
            filter &= f.Gte(r => r.ScheduledAt, from.Value);
        }

        if (to is not null)
        {
            filter &= f.Lte(r => r.ScheduledAt, to.Value);
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            // Escape the user's text so it is matched as plain text, not as a regex.
            var pattern = new BsonRegularExpression(Regex.Escape(text), "i");
            filter &= f.Or(
                f.Regex(r => r.StationName, pattern),
                f.Regex(r => r.ProsumerNic, pattern),
                f.Regex(r => r.Id, pattern));
        }

        return await _context.Reservations.Find(filter).SortBy(r => r.ScheduledAt).ToListAsync(cancellationToken);
    }

    // Inserts a new reservation.
    public async Task InsertAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.InsertOneAsync(reservation, cancellationToken: cancellationToken);
    }

    // Saves a changed reservation only if nobody else changed it since it was loaded.
    public async Task<bool> TryReplaceAsync(Reservation reservation, DateTime loadedUpdatedAt, CancellationToken cancellationToken = default)
    {
        var result = await _context.Reservations.ReplaceOneAsync(
            r => r.Id == reservation.Id && r.UpdatedAt == loadedUpdatedAt,
            reservation,
            cancellationToken: cancellationToken);

        return result.ModifiedCount == 1;
    }
}
