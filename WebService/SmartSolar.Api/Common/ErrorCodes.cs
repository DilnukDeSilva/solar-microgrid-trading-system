/*
 * File: ErrorCodes.cs
 * Description: Error `code` values returned in the contract `{ code, message }` body.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountNotActive = "ACCOUNT_NOT_ACTIVE";
    public const string UsernameExists = "USERNAME_EXISTS";
    public const string NicExists = "NIC_EXISTS";
    public const string RuleSevenDays = "RULE_7DAYS";
    public const string RuleTwelveHours = "RULE_12H";
    public const string SlotTaken = "SLOT_TAKEN";
    public const string SlotOverlap = "SLOT_OVERLAP";
    public const string InvalidState = "INVALID_STATE";
    public const string QrInvalid = "QR_INVALID";
    public const string QrAlreadyUsed = "QR_ALREADY_USED";
    public const string StationHasReservations = "STATION_HAS_RESERVATIONS";
    public const string InternalError = "INTERNAL_ERROR";
}
