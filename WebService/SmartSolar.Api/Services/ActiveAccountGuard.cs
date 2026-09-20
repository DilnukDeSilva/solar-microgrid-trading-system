/*
 * File: ActiveAccountGuard.cs
 * Description: Reads Users by id so JWT validation can reject deactivated or missing accounts.
 * Author: Member 1
 * Created: 21/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ActiveAccountGuard : IActiveAccountGuard
{
    private readonly IUserRepository _users;

    // Injects the shared user repository used on every authenticated request.
    public ActiveAccountGuard(IUserRepository users)
    {
        _users = users;
    }

    // Returns true only when the user exists and Status is Active.
    public async Task<bool> IsActiveAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        return user is not null && user.Status == UserStatuses.Active;
    }
}
