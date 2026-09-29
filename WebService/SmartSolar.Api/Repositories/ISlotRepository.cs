/*
 * File: ISlotRepository.cs
 * Description: Data-access contract for EnergyBookingSlots. Ready for Member 2 to extend.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public interface ISlotRepository
{
    // Returns every slot belonging to a station.
    Task<IReadOnlyList<Slot>> GetByStationAsync(string stationId, CancellationToken cancellationToken = default);

    // Loads one slot by id.
    Task<Slot?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    // Returns free slots of a station that start after `from` and no later than `to`.
    Task<IReadOnlyList<Slot>> GetFreeAsync(string stationId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    // Marks a free slot as taken. Returns false if someone else already has it.
    Task<bool> TryClaimAsync(string id, CancellationToken cancellationToken = default);

    // Marks a slot as free again.
    Task ReleaseAsync(string id, CancellationToken cancellationToken = default);
}
