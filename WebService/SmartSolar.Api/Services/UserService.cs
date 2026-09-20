/*
 * File: UserService.cs
 * Description: Backoffice user listing. Maps entities to DTOs so hashes never leave the API.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;

    // Injects the shared user repository owned by Member 1.
    public UserService(IUserRepository users)
    {
        _users = users;
    }

    // Returns every user without password hashes.
    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.GetAllAsync(cancellationToken);
        return users.Select(UserDto.From).ToList();
    }

    // Returns one user or 404 when the id does not exist.
    public async Task<UserDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "User was not found.");
        }

        return UserDto.From(user);
    }
}
