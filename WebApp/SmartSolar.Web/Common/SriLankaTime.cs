/*
 * File: SriLankaTime.cs
 * Description: Shows the API's UTC times in Sri Lanka time (UTC+05:30). Display only.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Common;

public static class SriLankaTime
{
    private static readonly TimeSpan Offset = new(5, 30, 0);

    // Converts a UTC time from the API to Sri Lanka local time.
    public static DateTime ToSriLanka(this DateTime utc)
    {
        return DateTime.SpecifyKind(utc.ToUniversalTime() + Offset, DateTimeKind.Unspecified);
    }

    // Formats a UTC time as e.g. "Mon 28 Sep 2026, 08:00".
    public static string ToSriLankaText(this DateTime utc)
    {
        return utc.ToSriLanka().ToString("ddd dd MMM yyyy, HH:mm");
    }
}
