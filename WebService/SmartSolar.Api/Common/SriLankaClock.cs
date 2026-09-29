/*
 * File: SriLankaClock.cs
 * Description: Sri Lanka (UTC+05:30) day boundaries for dashboard "today" and QR scan windows.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Api.Common;

public static class SriLankaClock
{
    private static readonly TimeSpan Offset = new(5, 30, 0);
    private static readonly TimeSpan NearWindow = TimeSpan.FromHours(2);

    // Returns the current UTC time used by booking rules.
    public static DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }

    // Returns the UTC start (inclusive) and end (exclusive) of the Sri Lanka calendar day containing utc.
    public static (DateTime StartUtc, DateTime EndUtc) DayRangeUtc(DateTime utc)
    {
        var local = DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(Offset);
        var startLocal = new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var startUtc = DateTime.SpecifyKind(startLocal.Subtract(Offset), DateTimeKind.Utc);
        return (startUtc, startUtc.AddDays(1));
    }

    // True when the slot is on today's Sri Lanka date, or within two hours of now.
    public static bool CanScanAt(DateTime scheduledAtUtc, DateTime nowUtc)
    {
        var (start, end) = DayRangeUtc(nowUtc);
        if (scheduledAtUtc >= start && scheduledAtUtc < end)
        {
            return true;
        }

        var delta = scheduledAtUtc - nowUtc;
        return delta <= NearWindow && delta >= -NearWindow;
    }
}
