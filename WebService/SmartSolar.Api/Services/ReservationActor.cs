/*
 * File: ReservationActor.cs
 * Description: The logged-in user making a reservation request, read from the JWT claims.
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Api.Common;

namespace SmartSolar.Api.Services;

public record ReservationActor(string UserId, string Role, string? Nic)
{
    public bool IsProsumer => Role == RoleNames.Prosumer;
}
