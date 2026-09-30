/*
 * File: OperatorService.cs
 * Description: Server-side QR checks. The token is opaque, single-use, and only valid near the booking time.
 * Author: samudith
 * Created: 29/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class OperatorService : IOperatorService
{
    private readonly IReservationQueryRepository _queries;
    private readonly IReservationRepository _reservations;

    // Injects the QR lookup and the reservation writer used to mark a job completed.
    public OperatorService(IReservationQueryRepository queries, IReservationRepository reservations)
    {
        _queries = queries;
        _reservations = reservations;
    }

    // Checks a QR token and returns the booking when it can be completed today.
    public async Task<ReservationDto> VerifyQrAsync(string? qrToken, CancellationToken cancellationToken = default)
    {
        var reservation = await FindTokenAsync(qrToken, cancellationToken);
        EnsureScannable(reservation, SriLankaClock.UtcNow());
        return ReservationDto.From(reservation);
    }

    // Marks an approved booking Completed. The token is kept so a second scan returns QR_ALREADY_USED.
    public async Task<ReservationDto> CompleteAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservations.FindByIdAsync(id, cancellationToken);
        if (reservation is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Reservation was not found.");
        }

        if (reservation.Status == ReservationStatuses.Completed)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrAlreadyUsed, "This QR code has already been used.");
        }

        if (reservation.Status != ReservationStatuses.Approved || string.IsNullOrWhiteSpace(reservation.QrToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.InvalidState, "Verify the QR code before completing this booking.");
        }

        var now = SriLankaClock.UtcNow();
        EnsureScannable(reservation, now);

        var loadedUpdatedAt = reservation.UpdatedAt;
        reservation.Status = ReservationStatuses.Completed;
        reservation.CompletedAt = now;
        reservation.CompletedBy = actor.UserId;
        reservation.UpdatedAt = now;

        if (!await _reservations.TryReplaceAsync(reservation, loadedUpdatedAt, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.InvalidState, "This reservation was just changed by someone else. Please reload and try again.");
        }

        return ReservationDto.From(reservation);
    }

    // Loads a token or rejects a blank or unknown code as QR_INVALID.
    private async Task<Models.Reservation> FindTokenAsync(string? qrToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(qrToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrInvalid, "The QR code is not valid.");
        }

        var reservation = await _queries.FindByQrTokenAsync(qrToken.Trim(), cancellationToken);
        if (reservation is null)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrInvalid, "The QR code is not valid.");
        }

        return reservation;
    }

    // Rejects a used, unapproved, or out-of-window booking. A completed token is QR_ALREADY_USED.
    private static void EnsureScannable(Models.Reservation reservation, DateTime nowUtc)
    {
        if (reservation.Status == ReservationStatuses.Completed)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrAlreadyUsed, "This QR code has already been used.");
        }

        if (reservation.Status != ReservationStatuses.Approved)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrInvalid, "This QR code is not valid for the booking's current status.");
        }

        if (!SriLankaClock.CanScanAt(reservation.ScheduledAt, nowUtc))
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.QrInvalid, "This QR code can only be used on the day of the booking.");
        }
    }
}
