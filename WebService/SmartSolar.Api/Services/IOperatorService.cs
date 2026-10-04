/*
 * File: IOperatorService.cs
 * Description: Grid-operator QR verification and job completion.
 * Author: Herath D M S T (IT22639776)
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IOperatorService
{
    // Checks a QR token and returns the booking when it can be completed today.
    Task<ReservationDto> VerifyQrAsync(string? qrToken, CancellationToken cancellationToken = default);

    // Marks an approved, verified booking Completed and keeps the token so a reuse is rejected.
    Task<ReservationDto> CompleteAsync(string id, ReservationActor actor, CancellationToken cancellationToken = default);
}
