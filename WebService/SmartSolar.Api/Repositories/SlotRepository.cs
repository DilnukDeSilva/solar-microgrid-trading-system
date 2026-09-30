/*
 * File: SlotRepository.cs
 * Description: MongoDB access for EnergyBookingSlots.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.Data;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public class SlotRepository : ISlotRepository
{
    private readonly MongoContext _context;

    // Stores the shared Mongo context used by slot queries.
    public SlotRepository(MongoContext context)
    {
        _context = context;
    }

    // Returns every slot belonging to a station.
    public async Task<IReadOnlyList<Slot>> GetByStationAsync(string stationId, CancellationToken cancellationToken = default)
    {
        return await _context.Slots.Find(slot => slot.StationId == stationId).ToListAsync(cancellationToken);
    }

    // Loads one slot by id.
    public async Task<Slot?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Slots.Find(slot => slot.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    // Returns free slots of a station that start after `from` and no later than `to`.
    public async Task<IReadOnlyList<Slot>> GetFreeAsync(string stationId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Slots
            .Find(slot => slot.StationId == stationId && slot.IsAvailable && slot.StartTime > from && slot.StartTime <= to)
            .SortBy(slot => slot.StartTime)
            .ToListAsync(cancellationToken);
    }

    // Marks a free slot as taken. Returns false if someone else already has it.
    public async Task<bool> TryClaimAsync(string id, CancellationToken cancellationToken = default)
    {
        // The filter and the update run as one operation in MongoDB, so only one request can win.
        var result = await _context.Slots.UpdateOneAsync(
            slot => slot.Id == id && slot.IsAvailable,
            Builders<Slot>.Update.Set(slot => slot.IsAvailable, false),
            cancellationToken: cancellationToken);

        return result.ModifiedCount == 1;
    }

    // Marks a slot as free again.
    public async Task ReleaseAsync(string id, CancellationToken cancellationToken = default)
    {
        await _context.Slots.UpdateOneAsync(
            slot => slot.Id == id,
            Builders<Slot>.Update.Set(slot => slot.IsAvailable, true),
            cancellationToken: cancellationToken);
    }

    // Inserts a new slot document.
    public Task InsertAsync(Slot slot, CancellationToken cancellationToken = default)
    {
        return _context.Slots.InsertOneAsync(slot, cancellationToken: cancellationToken);
    }

    // Replaces an existing slot document by id.
    public Task ReplaceAsync(Slot slot, CancellationToken cancellationToken = default)
    {
        return _context.Slots.ReplaceOneAsync(existing => existing.Id == slot.Id, slot, cancellationToken: cancellationToken);
    }

    // Deletes a slot document by id.
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _context.Slots.DeleteOneAsync(slot => slot.Id == id, cancellationToken);
    }

    // True when a Pending or Approved reservation still holds this slot.
    public Task<bool> HasActiveReservationAsync(string slotId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reservation>.Filter.Eq(reservation => reservation.SlotId, slotId)
            & Builders<Reservation>.Filter.In(
                reservation => reservation.Status,
                new[] { ReservationStatuses.Pending, ReservationStatuses.Approved });

        return _context.Reservations.Find(filter).AnyAsync(cancellationToken);
    }
}
