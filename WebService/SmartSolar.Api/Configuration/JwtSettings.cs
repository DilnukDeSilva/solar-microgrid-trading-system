/*
 * File: JwtSettings.cs
 * Description: JWT issuer/audience/key settings bound from configuration or environment.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    // Value in the committed *.example files. AddSmartSolarJwt refuses to use it as a real key.
    public const string PlaceholderKey = "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;

    public string Key { get; set; } = string.Empty;
}
