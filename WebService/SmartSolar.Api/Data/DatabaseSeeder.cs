/*
 * File: DatabaseSeeder.cs
 * Description: Idempotent seed of the contract sample users, stations, slots and reservations.
 * Author: Member 1
 * Created: 20/09/2026
 */

using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Data;

public class DatabaseSeeder
{
    public const string ActiveProsumerNic = "200012345678";
    public const string PendingProsumerNic = "199912345678";
    public const string MalabeStationId = "stn-malabe";
    public const string ColomboStationId = "stn-colombo-fort";
    public const string RajagiriyaStationId = "stn-rajagiriya";

    private readonly MongoContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly PasswordHasher<User> _passwordHasher = new();

    // Injects Mongo access used to insert sample documents when the database is empty.
    public DatabaseSeeder(MongoContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Seeds contract sample data once. Re-running is a no-op when Users already has documents.
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var userCount = await _context.Users.CountDocumentsAsync(FilterDefinition<User>.Empty, cancellationToken: cancellationToken);
        if (userCount > 0)
        {
            _logger.LogInformation("Seed skipped: Users already contains {Count} document(s).", userCount);
            return;
        }

        var createdAt = DateTime.UtcNow;
        var users = BuildUsers(createdAt);
        var stations = BuildStations();
        var slots = BuildSlots();
        var reservations = BuildReservations(slots, createdAt);

        foreach (var occupiedSlotId in reservations
                     .Where(r => r.Status is ReservationStatuses.Pending or ReservationStatuses.Approved or ReservationStatuses.Completed)
                     .Select(r => r.SlotId))
        {
            var slot = slots.First(s => s.Id == occupiedSlotId);
            slot.IsAvailable = false;
        }

        await _context.Users.InsertManyAsync(users, cancellationToken: cancellationToken);
        await _context.Stations.InsertManyAsync(stations, cancellationToken: cancellationToken);
        await _context.Slots.InsertManyAsync(slots, cancellationToken: cancellationToken);
        await _context.Reservations.InsertManyAsync(reservations, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Seed complete: {Users} users, {Stations} stations, {Slots} slots, {Reservations} reservations.",
            users.Count,
            stations.Count,
            slots.Count,
            reservations.Count);
    }

    // Builds the four contract users with hashed passwords (never store plaintext).
    private List<User> BuildUsers(DateTime createdAt)
    {
        var admin = NewUser("admin", null, "admin", "System Administrator", "admin@smartsolar.lk", "0770000001", RoleNames.Backoffice, UserStatuses.Active, createdAt);
        var operatorUser = NewUser("operator1", null, "operator1", "Grid Operator One", "operator1@smartsolar.lk", "0770000002", RoleNames.GridOperator, UserStatuses.Active, createdAt);
        var activeProsumer = NewUser(ActiveProsumerNic, ActiveProsumerNic, "nimal", "Nimal Perera", "nimal@smartsolar.lk", "0771234567", RoleNames.Prosumer, UserStatuses.Active, createdAt);
        var pendingProsumer = NewUser(PendingProsumerNic, PendingProsumerNic, "saman", "Saman Silva", "saman@smartsolar.lk", "0777654321", RoleNames.Prosumer, UserStatuses.Pending, createdAt);

        HashPassword(admin, "Admin@123");
        HashPassword(operatorUser, "Oper@123");
        HashPassword(activeProsumer, "Solar@123");
        HashPassword(pendingProsumer, "Solar@123");

        return new List<User> { admin, operatorUser, activeProsumer, pendingProsumer };
    }

    // Creates a user document. Prosumer id is the NIC as required by the plan.
    private static User NewUser(
        string id,
        string? nic,
        string username,
        string fullName,
        string email,
        string phone,
        string role,
        string status,
        DateTime createdAt)
    {
        return new User
        {
            Id = id,
            Nic = nic,
            Username = username,
            FullName = fullName,
            Email = email,
            Phone = phone,
            Role = role,
            Status = status,
            CreatedAt = createdAt
        };
    }

    // Hashes a seed password with ASP.NET Core Identity PasswordHasher.
    private void HashPassword(User user, string password)
    {
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
    }

    // Creates three Colombo/Malabe stations with a simple daily opening schedule.
    private static List<Station> BuildStations()
    {
        var week = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }
            .Select(day => new StationScheduleEntry { DayOfWeek = day, OpenTime = "08:00", CloseTime = "18:00" })
            .ToList();

        return new List<Station>
        {
            new()
            {
                Id = MalabeStationId,
                Name = "Malabe Solar Hub",
                Latitude = 6.9147,
                Longitude = 79.9730,
                CapacityKwh = 150,
                BatterySlotsTotal = 12,
                Schedule = week,
                Status = StationStatuses.Active
            },
            new()
            {
                Id = ColomboStationId,
                Name = "Colombo Fort Node",
                Latitude = 6.9339,
                Longitude = 79.8500,
                CapacityKwh = 200,
                BatterySlotsTotal = 16,
                Schedule = week,
                Status = StationStatuses.Active
            },
            new()
            {
                Id = RajagiriyaStationId,
                Name = "Rajagiriya Microgrid",
                Latitude = 6.9090,
                Longitude = 79.8910,
                CapacityKwh = 120,
                BatterySlotsTotal = 8,
                Schedule = week,
                Status = StationStatuses.Active
            }
        };
    }

    // Builds 4 slots per station for the next 5 local days, plus one historical slot for a completed booking.
    private static List<Slot> BuildSlots()
    {
        var tz = ColomboTimeZone();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var startDate = localNow.Date.AddDays(1);
        var windows = new (int StartHour, int EndHour)[]
        {
            (8, 10),
            (10, 12),
            (13, 15),
            (15, 17)
        };

        var stationIds = new[] { MalabeStationId, ColomboStationId, RajagiriyaStationId };
        var slots = new List<Slot>();

        for (var day = 0; day < 5; day++)
        {
            var date = startDate.AddDays(day);
            foreach (var stationId in stationIds)
            {
                for (var i = 0; i < windows.Length; i++)
                {
                    var (startHour, endHour) = windows[i];
                    var startLocal = DateTime.SpecifyKind(date.AddHours(startHour), DateTimeKind.Unspecified);
                    var endLocal = DateTime.SpecifyKind(date.AddHours(endHour), DateTimeKind.Unspecified);
                    slots.Add(new Slot
                    {
                        Id = $"slot-{stationId}-{date:yyyyMMdd}-{i + 1}",
                        StationId = stationId,
                        StartTime = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz),
                        EndTime = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz),
                        IsAvailable = true
                    });
                }
            }
        }

        var yesterday = localNow.Date.AddDays(-1);
        var pastStart = DateTime.SpecifyKind(yesterday.AddHours(8), DateTimeKind.Unspecified);
        var pastEnd = DateTime.SpecifyKind(yesterday.AddHours(10), DateTimeKind.Unspecified);
        slots.Add(new Slot
        {
            Id = $"slot-{MalabeStationId}-{yesterday:yyyyMMdd}-past",
            StationId = MalabeStationId,
            StartTime = TimeZoneInfo.ConvertTimeToUtc(pastStart, tz),
            EndTime = TimeZoneInfo.ConvertTimeToUtc(pastEnd, tz),
            IsAvailable = false
        });

        return slots;
    }

    // Seeds one reservation in each status, including a completed booking in the past.
    private static List<Reservation> BuildReservations(IReadOnlyList<Slot> slots, DateTime createdAt)
    {
        var malabeTomorrow = slots
            .Where(s => s.StationId == MalabeStationId && s.Id.Contains("-past") == false)
            .OrderBy(s => s.StartTime)
            .Take(3)
            .ToList();

        var pastSlot = slots.First(s => s.Id.EndsWith("-past", StringComparison.Ordinal));
        var qrToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

        return new List<Reservation>
        {
            new()
            {
                Id = "res-pending-1",
                ProsumerNic = ActiveProsumerNic,
                StationId = MalabeStationId,
                StationName = "Malabe Solar Hub",
                SlotId = malabeTomorrow[0].Id,
                ScheduledAt = malabeTomorrow[0].StartTime,
                Status = ReservationStatuses.Pending,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new()
            {
                Id = "res-approved-1",
                ProsumerNic = ActiveProsumerNic,
                StationId = MalabeStationId,
                StationName = "Malabe Solar Hub",
                SlotId = malabeTomorrow[1].Id,
                ScheduledAt = malabeTomorrow[1].StartTime,
                Status = ReservationStatuses.Approved,
                QrToken = qrToken,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new()
            {
                Id = "res-cancelled-1",
                ProsumerNic = ActiveProsumerNic,
                StationId = MalabeStationId,
                StationName = "Malabe Solar Hub",
                SlotId = malabeTomorrow[2].Id,
                ScheduledAt = malabeTomorrow[2].StartTime,
                Status = ReservationStatuses.Cancelled,
                CreatedAt = createdAt.AddDays(-2),
                UpdatedAt = createdAt
            },
            new()
            {
                Id = "res-completed-past-1",
                ProsumerNic = ActiveProsumerNic,
                StationId = MalabeStationId,
                StationName = "Malabe Solar Hub",
                SlotId = pastSlot.Id,
                ScheduledAt = pastSlot.StartTime,
                Status = ReservationStatuses.Completed,
                CreatedAt = pastSlot.StartTime.AddDays(-1),
                UpdatedAt = pastSlot.EndTime,
                CompletedAt = pastSlot.EndTime,
                CompletedBy = "operator1"
            }
        };
    }

    // Resolves Sri Lanka time on both macOS (IANA) and Windows (system name).
    private static TimeZoneInfo ColomboTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
        }
    }
}
