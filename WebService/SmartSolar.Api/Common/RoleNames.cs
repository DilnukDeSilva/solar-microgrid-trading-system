/*
 * File: RoleNames.cs
 * Description: Role strings used by JWT claims and [Authorize(Roles)] attributes.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Common;

public static class RoleNames
{
    public const string Backoffice = "Backoffice";
    public const string GridOperator = "GridOperator";
    public const string Prosumer = "Prosumer";
}
