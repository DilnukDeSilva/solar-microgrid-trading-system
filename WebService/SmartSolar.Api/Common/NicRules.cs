/*
 * File: NicRules.cs
 * Description: Sri Lankan NIC format used for prosumer primary keys. Rules live in the service layer.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

using System.Text.RegularExpressions;

namespace SmartSolar.Api.Common;

public static class NicRules
{
    private static readonly Regex OldNic = new(@"^\d{9}[VX]$", RegexOptions.Compiled);
    private static readonly Regex NewNic = new(@"^\d{12}$", RegexOptions.Compiled);

    // Trims and uppercases a NIC so V/X and stored keys match.
    public static string Normalize(string nic)
    {
        return nic.Trim().ToUpperInvariant();
    }

    // Returns true for the old 9-digit+V/X form or the 12-digit form.
    public static bool IsValid(string nic)
    {
        var value = Normalize(nic);
        return OldNic.IsMatch(value) || NewNic.IsMatch(value);
    }

    // Throws VALIDATION_ERROR when the NIC is missing or not a recognised Sri Lankan format.
    public static void RequireValid(string? nic)
    {
        if (string.IsNullOrWhiteSpace(nic) || !IsValid(nic))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "NIC must be 9 digits plus V or X, or 12 digits.");
        }
    }
}
