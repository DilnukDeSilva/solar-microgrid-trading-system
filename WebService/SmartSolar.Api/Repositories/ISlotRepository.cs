/*
 * File: ISlotRepository.cs
 * Description: Data-access contract for EnergyBookingSlots. Ready for Member 2 to extend.
 * Author: Member 1
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
}
