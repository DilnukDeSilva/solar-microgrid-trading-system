/*
 * File: ReservationRepository.cs
 * Description: MongoDB access for EnergyReservation.
 * Author: Member 1
 * Created: 20/09/2026
 */

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
}
