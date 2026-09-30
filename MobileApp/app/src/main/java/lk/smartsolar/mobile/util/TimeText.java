/*
 * File: TimeText.java
 * Description: Shows API UTC timestamps in Sri Lanka time. Display only.
 * Author: samudith
 * Created: 29/09/2026
 */
package lk.smartsolar.mobile.util;

import java.time.OffsetDateTime;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.Date;

public final class TimeText {
    private static final DateTimeFormatter FORMAT = DateTimeFormatter.ofPattern("EEE dd MMM yyyy, HH:mm");

    private TimeText() {
    }

    // Formats an ISO-8601 UTC string, or returns it unchanged when it cannot be parsed.
    public static String sriLanka(String iso) {
        if (iso == null || iso.isEmpty()) return "";
        try {
            return OffsetDateTime.parse(iso).atZoneSameInstant(ZoneId.of("Asia/Colombo")).format(FORMAT);
        } catch (Exception ignored) {
            return iso;
        }
    }

    // Formats a millisecond timestamp as a local "last updated" label.
    public static String lastUpdated(long millis) {
        if (millis <= 0) return "Not synced yet";
        return "Last updated " + android.text.format.DateFormat.format("dd MMM yyyy, HH:mm", new Date(millis));
    }
}
