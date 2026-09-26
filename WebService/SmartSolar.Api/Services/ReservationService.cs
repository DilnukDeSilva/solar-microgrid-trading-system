/*
 * File: ReservationService.cs
 * Description: Reservation business rules (7-day window, slot availability, active accounts and ownership).
 * Author: Janukshan S (IT22635266)
 */

using MongoDB.Bson;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ReservationService : IReservationService
{
    public const int BookingWindowDays = 7;

    private readonly IReservationRepository _reservations;
    private readonly ISlotRepository _slots;
    private readonly IStationRepository _stations;
    private readonly IUserRepository _users;

    // Injects the repositories used by the reservation rules.
    public ReservationService(
        IReservationRepository reservations,
        ISlotRepository slots,
        IStationRepository stations,
        IUserRepository users)
    {
        _reservations = reservations;
        _slots = slots;
        _stations = stations;
        _users = users;
    }

    // Books a free slot for a prosumer, or for the given NIC when staff book on their behalf.
    public async Task<ReservationDto> CreateAsync(CreateReservationRequestDto request, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        RequireText(request.StationId, "Station is required.");
        RequireText(request.SlotId, "Slot is required.");

        // A prosumer always books for themselves, so their NIC comes from the token and not the body.
        var nic = actor.IsProsumer ? actor.Nic : request.ProsumerNic?.Trim();
        if (actor.IsProsumer && string.IsNullOrWhiteSpace(nic))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, "Your account has no NIC linked to it.");
        }

        RequireText(nic, "Prosumer NIC is required when booking on behalf of a prosumer.");

        var prosumer = await LoadProsumerAsync(nic!, cancellationToken);
        var station = await LoadStationAsync(request.StationId.Trim(), cancellationToken);
        var slot = await LoadSlotAsync(request.SlotId.Trim(), station.Id, cancellationToken);

        var now = DateTime.UtcNow;
        EnsureWithinBookingWindow(slot.StartTime, now);
        EnsureProsumerActive(prosumer);
        EnsureStationActive(station);

        if (!await _slots.TryClaimAsync(slot.Id, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.SlotTaken, "This slot has already been booked. Please pick another one.");
        }

        var reservation = new Reservation
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProsumerNic = prosumer.Id,
            StationId = station.Id,
            StationName = station.Name,
            SlotId = slot.Id,
            ScheduledAt = slot.StartTime,
            Status = ReservationStatuses.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actor.IsProsumer ? null : actor.UserId
        };

        try
        {
            await _reservations.InsertAsync(reservation, cancellationToken);
        }
        catch
        {
            // The slot was claimed but the booking was not saved, so give the slot back.
            await _slots.ReleaseAsync(slot.Id, CancellationToken.None);
            throw;
        }

        return ReservationDto.From(reservation);
    }

    // Returns one reservation. Prosumers can only read their own.
    public async Task<ReservationDto> GetByIdAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadReservationAsync(id, cancellationToken);
        EnsureCanAccess(reservation, actor);
        return ReservationDto.From(reservation);
    }

    // Loads a reservation or throws 404.
    private async Task<Reservation> LoadReservationAsync(string id, CancellationToken cancellationToken)
    {
        var reservation = await _reservations.FindByIdAsync(id, cancellationToken);
        if (reservation is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Reservation was not found.");
        }

        return reservation;
    }

    // Loads a prosumer by NIC or throws 404.
    private async Task<User> LoadProsumerAsync(string nic, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(nic, cancellationToken);
        if (user is null || user.Role != RoleNames.Prosumer)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "No prosumer was found with that NIC.");
        }

        return user;
    }

    // Loads a station or throws 404.
    private async Task<Station> LoadStationAsync(string stationId, CancellationToken cancellationToken)
    {
        var station = await _stations.FindByIdAsync(stationId, cancellationToken);
        if (station is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Station was not found.");
        }

        return station;
    }

    // Loads a slot and checks it belongs to the chosen station.
    private async Task<Slot> LoadSlotAsync(string slotId, string stationId, CancellationToken cancellationToken)
    {
        var slot = await _slots.FindByIdAsync(slotId, cancellationToken);
        if (slot is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Slot was not found.");
        }

        if (slot.StationId != stationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "The selected slot does not belong to this station.");
        }

        return slot;
    }

    // Slot must start in the future and no later than 7 days from now.
    private static void EnsureWithinBookingWindow(DateTime slotStart, DateTime now)
    {
        if (slotStart <= now || slotStart > now.AddDays(BookingWindowDays))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.RuleSevenDays,
                $"Reservations can only be made for a future slot within the next {BookingWindowDays} days.");
        }
    }

    // Only Active prosumers can hold bookings.
    private static void EnsureProsumerActive(User prosumer)
    {
        if (prosumer.Status != UserStatuses.Active)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.AccountNotActive, "This prosumer account is not active.");
        }
    }

    // Deactivated stations cannot take new bookings.
    private static void EnsureStationActive(Station station)
    {
        if (station.Status != StationStatuses.Active)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "This station is not accepting bookings.");
        }
    }

    // A prosumer may only touch their own reservations. Staff can see all.
    private static void EnsureCanAccess(Reservation reservation, ReservationActor actor)
    {
        if (actor.IsProsumer && reservation.ProsumerNic != actor.Nic)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, "You can only access your own reservations.");
        }
    }

    // Throws 400 when a required value is blank.
    private static void RequireText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, message);
        }
    }
}
