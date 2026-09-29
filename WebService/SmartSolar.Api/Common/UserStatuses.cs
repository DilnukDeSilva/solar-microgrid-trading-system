/*
 * File: UserStatuses.cs
 * Description: Allowed values for Users.status, stored as strings in MongoDB.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Common;

public static class UserStatuses
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Deactivated = "Deactivated";
}
