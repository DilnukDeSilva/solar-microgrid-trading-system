/*
 * File: ReservationService.cs
 * Description: Reservation business rules: 7-day window, 12-hour notice, slot availability, status changes and QR approval.
 * Author: Janukshan S (IT22635266)
 */

using System.Buffers.Text;
using System.Security.Cryptography;
using MongoDB.Bson;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ReservationService : IReservationService
{
    public const int BookingWindowDays = 7;
    public const int ChangeNoticeHours = 12;

    // Sri Lanka has no daylight saving, so local time is always UTC+05:30.
    private static readonly TimeSpan SriLankaOffset = new(5, 30, 0);

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

    // Returns the stations that are taking bookings.
    public async Task<IReadOnlyList<BookingStationDto>> GetBookableStationsAsync(CancellationToken cancellationToken = default)
    {
        var stations = await _stations.GetAllAsync(cancellationToken);
        return stations
            .Where(station => station.Status == StationStatuses.Active)
            .OrderBy(station => station.Name)
            .Select(station => new BookingStationDto { Id = station.Id, Name = station.Name })
            .ToList();
    }

    // Returns free slots of a station inside the booking window, optionally for one Sri Lanka date.
    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(string stationId, DateOnly? date, CancellationToken cancellationToken = default)
    {
        var station = await LoadStationAsync(stationId, cancellationToken);
        EnsureStationActive(station);

        var now = DateTime.UtcNow;
        var from = now;
        var to = now.AddDays(BookingWindowDays);

        if (date is not null)
        {
            // The client picks a local date, so convert that day's start and end to UTC.
            var dayStart = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - SriLankaOffset;
            from = dayStart > from ? dayStart : from;
            to = dayStart.AddDays(1) < to ? dayStart.AddDays(1) : to;
        }

        if (from >= to)
        {
            return Array.Empty<AvailableSlotDto>();
        }

        var slots = await _slots.GetFreeAsync(station.Id, from, to, cancellationToken);
        return slots.Select(AvailableSlotDto.From).ToList();
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

        var now = DateTime.UtcNow;
        var prosumer = await LoadProsumerAsync(nic!, cancellationToken);
        var (station, slot) = await LoadBookableSlotAsync(request.StationId.Trim(), request.SlotId.Trim(), now, cancellationToken);
        EnsureProsumerActive(prosumer);

        await ClaimSlotAsync(slot.Id, cancellationToken);

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

    // Moves a booking to another free slot. Needs 12 hours notice.
    public async Task<ReservationDto> UpdateAsync(string id, UpdateReservationRequestDto request, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        RequireText(request.SlotId, "Slot is required.");

        var reservation = await LoadReservationAsync(id, cancellationToken);
        EnsureCanAccess(reservation, actor);
        EnsureChangeable(reservation);

        var now = DateTime.UtcNow;
        EnsureTwelveHoursNotice(reservation, now);

        var stationId = string.IsNullOrWhiteSpace(request.StationId) ? reservation.StationId : request.StationId.Trim();
        var slotId = request.SlotId.Trim();
        if (slotId == reservation.SlotId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Please choose a different slot from the one already booked.");
        }

        var (station, slot) = await LoadBookableSlotAsync(stationId, slotId, now, cancellationToken);
        await ClaimSlotAsync(slot.Id, cancellationToken);

        var oldSlotId = reservation.SlotId;
        var loadedUpdatedAt = reservation.UpdatedAt;

        reservation.StationId = station.Id;
        reservation.StationName = station.Name;
        reservation.SlotId = slot.Id;
        reservation.ScheduledAt = slot.StartTime;
        reservation.UpdatedAt = now;

        // A changed booking needs to be approved again, and the old QR must stop working.
        reservation.Status = ReservationStatuses.Pending;
        reservation.QrToken = null;
        reservation.ApprovedAt = null;
        reservation.ApprovedBy = null;

        var saved = false;
        try
        {
            saved = await _reservations.TryReplaceAsync(reservation, loadedUpdatedAt, cancellationToken);
        }
        finally
        {
            if (!saved)
            {
                await _slots.ReleaseAsync(slot.Id, CancellationToken.None);
            }
        }

        if (!saved)
        {
            throw ChangedBySomeoneElse();
        }

        await _slots.ReleaseAsync(oldSlotId, CancellationToken.None);
        return ReservationDto.From(reservation);
    }

    // Cancels a booking and frees its slot. Needs 12 hours notice.
    public async Task<ReservationDto> CancelAsync(string id, CancelReservationRequestDto? request, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadReservationAsync(id, cancellationToken);
        EnsureCanAccess(reservation, actor);
        EnsureChangeable(reservation);

        var now = DateTime.UtcNow;
        EnsureTwelveHoursNotice(reservation, now);

        var reason = request?.Reason?.Trim();
        var loadedUpdatedAt = reservation.UpdatedAt;
        reservation.Status = ReservationStatuses.Cancelled;
        reservation.QrToken = null;
        reservation.CancelledAt = now;
        reservation.CancelledBy = actor.UserId;
        reservation.CancelReason = string.IsNullOrEmpty(reason) ? null : reason;
        reservation.UpdatedAt = now;

        if (!await _reservations.TryReplaceAsync(reservation, loadedUpdatedAt, cancellationToken))
        {
            throw ChangedBySomeoneElse();
        }

        await _slots.ReleaseAsync(reservation.SlotId, CancellationToken.None);
        return ReservationDto.From(reservation);
    }

    // Approves a pending booking and gives it a QR token.
    public async Task<ReservationDto> ApproveAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadReservationAsync(id, cancellationToken);
        if (reservation.Status != ReservationStatuses.Pending)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.InvalidState,
                $"Only pending reservations can be approved. This one is {reservation.Status}.");
        }

        var now = DateTime.UtcNow;
        if (reservation.ScheduledAt <= now)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.InvalidState, "This reservation's slot has already started, so it cannot be approved.");
        }

        var loadedUpdatedAt = reservation.UpdatedAt;
        reservation.Status = ReservationStatuses.Approved;
        reservation.QrToken = NewQrToken();
        reservation.ApprovedAt = now;
        reservation.ApprovedBy = actor.UserId;
        reservation.UpdatedAt = now;

        if (!await _reservations.TryReplaceAsync(reservation, loadedUpdatedAt, cancellationToken))
        {
            throw ChangedBySomeoneElse();
        }

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

    // Loads the station and slot for a new booking and checks both can be booked.
    private async Task<(Station Station, Slot Slot)> LoadBookableSlotAsync(string stationId, string slotId, DateTime now, CancellationToken cancellationToken)
    {
        var station = await LoadStationAsync(stationId, cancellationToken);
        var slot = await LoadSlotAsync(slotId, station.Id, cancellationToken);
        EnsureWithinBookingWindow(slot.StartTime, now);
        EnsureStationActive(station);
        return (station, slot);
    }

    // Takes the slot, or throws SLOT_TAKEN if another booking got it first.
    private async Task ClaimSlotAsync(string slotId, CancellationToken cancellationToken)
    {
        if (!await _slots.TryClaimAsync(slotId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.SlotTaken, "This slot has already been booked. Please pick another one.");
        }
    }

    // Completed and cancelled bookings are final.
    private static void EnsureChangeable(Reservation reservation)
    {
        if (reservation.Status is not (ReservationStatuses.Pending or ReservationStatuses.Approved))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.InvalidState,
                $"This reservation is {reservation.Status} and can no longer be changed.");
        }
    }

    // Updates and cancellations need at least 12 hours before the booked slot starts.
    private static void EnsureTwelveHoursNotice(Reservation reservation, DateTime now)
    {
        if (reservation.ScheduledAt - now < TimeSpan.FromHours(ChangeNoticeHours))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.RuleTwelveHours,
                $"Reservations can only be changed or cancelled at least {ChangeNoticeHours} hours before the slot starts.");
        }
    }

    // Error for when another request changed the reservation first.
    private static ApiException ChangedBySomeoneElse()
    {
        return new ApiException(StatusCodes.Status409Conflict, ErrorCodes.InvalidState, "This reservation was just changed by someone else. Please reload and try again.");
    }

    // Makes a random, URL-safe token for the QR code. It holds no personal data.
    private static string NewQrToken()
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
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
