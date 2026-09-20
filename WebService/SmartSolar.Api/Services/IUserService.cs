/*
 * File: IUserService.cs
 * Description: Staff user use-cases for Backoffice. Prosumer admin endpoints belong to Member 2.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IUserService
{
    // Returns staff accounts without password hashes.
    Task<IReadOnlyList<UserDto>> ListStaffAsync(CancellationToken cancellationToken = default);

    // Returns one staff user or throws 404 when the id is missing or is not staff.
    Task<UserDto> GetStaffByIdAsync(string id, CancellationToken cancellationToken = default);

    // Creates a Backoffice or GridOperator account.
    Task<UserDto> CreateStaffAsync(CreateStaffRequestDto request, CancellationToken cancellationToken = default);

    // Updates a staff account. Password is optional.
    Task<UserDto> UpdateStaffAsync(string id, UpdateStaffRequestDto request, CancellationToken cancellationToken = default);

    // Sets a staff account to Deactivated. Callers pass the current user id to block self-deactivation.
    Task<UserDto> DeactivateStaffAsync(string id, string? actorId, CancellationToken cancellationToken = default);
}
