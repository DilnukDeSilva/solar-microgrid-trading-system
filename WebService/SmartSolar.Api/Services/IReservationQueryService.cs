/*
 * File: IReservationQueryService.cs
 * Description: Reservation list and search. Owned by Member 4; stand-in written by Janukshan S (IT22635266) so the web list works before merge.
 * Author: Member 4
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IReservationQueryService
{
    // Lists reservations with optional filters. Prosumers only get their own.
    Task<IReadOnlyList<ReservationDto>> SearchAsync(string? status, DateTime? from, DateTime? to, string? q, string? nic, ReservationActor actor, CancellationToken cancellationToken = default);
}
