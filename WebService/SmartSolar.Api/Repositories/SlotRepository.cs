/*
 * File: SlotRepository.cs
 * Description: MongoDB access for EnergyBookingSlots.
 * Author: Member 1
 * Created: 20/09/2026
 */

using MongoDB.Driver;
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
}
