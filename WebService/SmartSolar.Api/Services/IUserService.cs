/*
 * File: IUserService.cs
 * Description: Staff user queries for Backoffice. Full CRUD lands in Milestone 1.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IUserService
{
    // Returns every user without password hashes.
    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken = default);

    // Returns one user or throws 404 when the id does not exist.
    Task<UserDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
