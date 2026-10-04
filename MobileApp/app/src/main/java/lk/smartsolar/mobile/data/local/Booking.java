/*
 * File: Booking.java
 * Description: One reservation row used by the bookings list and the offline cache.
 * Author: Herath D M S T (IT22639776)
 */

package lk.smartsolar.mobile.data.local;

import org.json.JSONObject;

public class Booking {
    public String id = "";
    public String prosumerNic = "";
    public String stationName = "";
    public String scheduledAt = "";
    public String status = "";
    public long syncedAt;

    // Reads the contract reservation fields used on the bookings screen.
    public static Booking fromJson(JSONObject json) {
        Booking booking = new Booking();
        booking.id = json.optString("id", "");
        booking.prosumerNic = json.optString("prosumerNic", "");
        booking.stationName = json.optString("stationName", "");
        booking.scheduledAt = json.optString("scheduledAt", "");
        booking.status = json.optString("status", "");
        return booking;
    }
}
