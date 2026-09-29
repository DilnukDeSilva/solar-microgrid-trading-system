/*
 * File: ReservationQueryService.cs
 * Description: Reservation list and search. Owned by Member 4; stand-in written by Janukshan S (IT22635266) so the web list works before merge.
 * Author: Member 4
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ReservationQueryService : IReservationQueryService
{
    private static readonly string[] Statuses =
    [
        ReservationStatuses.Pending,
        ReservationStatuses.Approved,
        ReservationStatuses.Cancelled,
        ReservationStatuses.Completed
    ];

    private readonly IReservationRepository _reservations;

    // Injects the reservation repository.
    public ReservationQueryService(IReservationRepository reservations)
    {
        _reservations = reservations;
    }

    // Lists reservations with optional filters. Prosumers only get their own.
    public async Task<IReadOnlyList<ReservationDto>> SearchAsync(string? status, DateTime? from, DateTime? to, string? q, string? nic, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(status) && !Statuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Status must be Pending, Approved, Cancelled or Completed.");
        }

        var nicFilter = actor.IsProsumer ? actor.Nic : nic?.Trim();
        var results = await _reservations.SearchAsync(
            nicFilter,
            status,
            from?.ToUniversalTime(),
            to?.ToUniversalTime(),
            q?.Trim(),
            cancellationToken);

        return results.Select(ReservationDto.From).ToList();
    }
}
