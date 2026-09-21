/*
 * File: IActiveAccountGuard.cs
 * Description: Looks up the current user status during JWT validation. Deactivated tokens must fail.
 * Author: Member 1
 * Created: 21/09/2026
 */

namespace SmartSolar.Api.Services;

public interface IActiveAccountGuard
{
    // Returns true only when the user exists and Status is Active.
    Task<bool> IsActiveAsync(string? userId, CancellationToken cancellationToken = default);
}
