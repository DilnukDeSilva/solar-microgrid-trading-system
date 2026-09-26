/*
 * File: IReservationService.cs
 * Description: Reservation use cases. All booking rules for web and mobile run behind this interface.
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IReservationService
{
    // Returns the stations that are taking bookings.
    Task<IReadOnlyList<BookingStationDto>> GetBookableStationsAsync(CancellationToken cancellationToken = default);

    // Returns free slots of a station inside the booking window, optionally for one Sri Lanka date.
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(string stationId, DateOnly? date, CancellationToken cancellationToken = default);

    // Books a free slot for a prosumer, or for the given NIC when staff book on their behalf.
    Task<ReservationDto> CreateAsync(CreateReservationRequestDto request, ReservationActor actor, CancellationToken cancellationToken = default);

    // Returns one reservation. Prosumers can only read their own.
    Task<ReservationDto> GetByIdAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default);

    // Moves a booking to another free slot. Needs 12 hours notice.
    Task<ReservationDto> UpdateAsync(string id, UpdateReservationRequestDto request, ReservationActor actor, CancellationToken cancellationToken = default);

    // Cancels a booking and frees its slot. Needs 12 hours notice.
    Task<ReservationDto> CancelAsync(string id, CancelReservationRequestDto? request, ReservationActor actor, CancellationToken cancellationToken = default);

    // Approves a pending booking and gives it a QR token.
    Task<ReservationDto> ApproveAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default);
}
