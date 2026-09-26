/*
 * File: IReservationRepository.cs
 * Description: Data-access contract for EnergyReservation. Ready for Members 2 and 4 to extend.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public interface IReservationRepository
{
    // Returns every reservation document.
    Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken = default);

    // Loads one reservation by id.
    Task<Reservation?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    // Inserts a new reservation.
    Task InsertAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
