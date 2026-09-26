/*
 * File: MongoIndexInitializer.cs
 * Description: Creates unique NIC/username indexes and reservation lookup indexes on startup.
 * Author: Member 1
 * Created: 20/09/2026
 */

using MongoDB.Driver;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Data;

public class MongoIndexInitializer
{
    private readonly MongoContext _context;
    private readonly ILogger<MongoIndexInitializer> _logger;

    // Injects the shared Mongo context used to create indexes.
    public MongoIndexInitializer(MongoContext context, ILogger<MongoIndexInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Ensures the four collections exist and have the indexes required by the contract.
    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var nicIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(user => user.Nic),
            new CreateIndexOptions
            {
                Unique = true,
                Sparse = true,
                Name = "ux_users_nic"
            });

        var usernameIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(user => user.Username),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_users_username"
            });

        await _context.Users.Indexes.CreateManyAsync(new[] { nicIndex, usernameIndex }, cancellationToken);

        await _context.Reservations.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.ProsumerNic),
                    new CreateIndexOptions { Name = "ix_reservations_prosumerNic" }),
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.ScheduledAt),
                    new CreateIndexOptions { Name = "ix_reservations_scheduledAt" }),
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.Status),
                    new CreateIndexOptions { Name = "ix_reservations_status" }),
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.ProsumerNic).Ascending(r => r.ScheduledAt),
                    new CreateIndexOptions { Name = "ix_reservations_prosumerNic_scheduledAt" }),
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.Status).Ascending(r => r.ScheduledAt),
                    new CreateIndexOptions { Name = "ix_reservations_status_scheduledAt" }),
                new CreateIndexModel<Reservation>(
                    Builders<Reservation>.IndexKeys.Ascending(r => r.QrToken),
                    new CreateIndexOptions { Name = "ux_reservations_qrToken", Unique = true, Sparse = true })
            },
            cancellationToken);

        // Touch the remaining collections so Compass shows all four after first boot.
        await _context.Stations.Indexes.CreateOneAsync(
            new CreateIndexModel<Station>(Builders<Station>.IndexKeys.Ascending(s => s.Status), new CreateIndexOptions { Name = "ix_stations_status" }),
            cancellationToken: cancellationToken);

        await _context.Slots.Indexes.CreateOneAsync(
            new CreateIndexModel<Slot>(Builders<Slot>.IndexKeys.Ascending(s => s.StationId), new CreateIndexOptions { Name = "ix_slots_stationId" }),
            cancellationToken: cancellationToken);

        _logger.LogInformation("MongoDB indexes ensured for Users, SolarStationInfo, EnergyBookingSlots and EnergyReservation.");
    }
}
