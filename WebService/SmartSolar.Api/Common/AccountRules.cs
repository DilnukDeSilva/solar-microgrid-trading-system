/*
 * File: AccountRules.cs
 * Description: Shared field checks for staff and prosumer accounts. The API is the only place these run.
 * Author: Dilnuk De Silva
 * Created: 27/09/2026
 */

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartSolar.Api.Common;

public static class AccountRules
{
    public const int MinPasswordLength = 8;

    private static readonly Regex PhonePattern = new(@"^(\+94\d{9}|0\d{9}|\d{9,12})$", RegexOptions.Compiled);

    // Throws 400 when a required string is blank.
    public static void RequireText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, message);
        }
    }

    // Rejects a non-blank value that is not an email address. Blank is allowed.
    public static void RequireEmailFormat(string? email)
    {
        var trimmed = email?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return;
        }

        if (!new EmailAddressAttribute().IsValid(trimmed))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Email is not a valid address.");
        }
    }

    // Rejects passwords shorter than the service minimum. Clients are not trusted.
    public static void RequirePasswordLength(string password)
    {
        if (password.Length < MinPasswordLength)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                $"Password must be at least {MinPasswordLength} characters.");
        }
    }

    // Accepts blank phone, otherwise requires a Sri Lankan-style or 9–12 digit number.
    public static void RequirePhoneFormat(string? phone)
    {
        var trimmed = phone?.Trim().Replace(" ", string.Empty) ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return;
        }

        if (!PhonePattern.IsMatch(trimmed))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Phone must be 10 digits starting with 0, or +94 followed by 9 digits.");
        }
    }
}
