/*
 * File: IReservationService.cs
 * Description: Reservation use cases. All booking rules for web and mobile run behind this interface.
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IReservationService
{
    // Books a free slot for a prosumer, or for the given NIC when staff book on their behalf.
    Task<ReservationDto> CreateAsync(CreateReservationRequestDto request, ReservationActor actor, CancellationToken cancellationToken = default);

    // Returns one reservation. Prosumers can only read their own.
    Task<ReservationDto> GetByIdAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default);
}
