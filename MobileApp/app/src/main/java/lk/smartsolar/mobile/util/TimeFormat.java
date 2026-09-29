/*
 * File: TimeFormat.java
 * Description: Shows the API's UTC times in Sri Lanka time. Display only, no booking rules.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.util;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

public final class TimeFormat {

    private static final TimeZone SRI_LANKA = TimeZone.getTimeZone("Asia/Colombo");

    private TimeFormat() {
    }

    // Parses an API time like "2026-09-28T02:30:00Z" or "2026-09-28T02:30:00.123Z". Seconds fractions are dropped.
    public static Date parse(String iso) {
        if (iso == null || iso.length() < 19) {
            return null;
        }
        SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US);
        format.setTimeZone(TimeZone.getTimeZone("UTC"));
        try {
            return format.parse(iso.substring(0, 19));
        } catch (ParseException e) {
            return null;
        }
    }

    // e.g. "Mon 28 Sep 2026, 08:00"
    public static String full(String iso) {
        return format(iso, "EEE dd MMM yyyy, HH:mm");
    }

    // e.g. "Mon 28 Sep"
    public static String day(String iso) {
        return format(iso, "EEE dd MMM");
    }

    // e.g. "08:00"
    public static String time(String iso) {
        return format(iso, "HH:mm");
    }

    // Formats a UTC API time in Sri Lanka time with the given pattern.
    private static String format(String iso, String pattern) {
        Date date = parse(iso);
        if (date == null) {
            return "";
        }
        SimpleDateFormat format = new SimpleDateFormat(pattern, Locale.US);
        format.setTimeZone(SRI_LANKA);
        return format.format(date);
    }
}
