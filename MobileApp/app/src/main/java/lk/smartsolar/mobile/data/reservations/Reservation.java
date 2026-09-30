/*
 * File: Reservation.java
 * Description: A reservation as returned by the API.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.data.reservations;

import org.json.JSONObject;

public class Reservation {

    public String id;
    public String prosumerNic;
    public String stationId;
    public String stationName;
    public String slotId;
    public String scheduledAt;
    public String status;
    public String qrToken;
    public long syncedAt;

    // Reads the fields the app uses from the API's JSON.
    public static Reservation fromJson(JSONObject json) {
        Reservation r = new Reservation();
        r.id = json.optString("id");
        r.prosumerNic = json.optString("prosumerNic");
        r.stationId = json.optString("stationId");
        r.stationName = json.optString("stationName");
        r.slotId = json.optString("slotId");
        r.scheduledAt = json.optString("scheduledAt");
        r.status = json.optString("status");
        r.qrToken = json.isNull("qrToken") ? null : json.optString("qrToken", null);
        return r;
    }
}
