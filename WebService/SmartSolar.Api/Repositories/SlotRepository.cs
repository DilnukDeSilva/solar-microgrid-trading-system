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
}
