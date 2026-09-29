/*
 * File: DashboardSnapshot.java
 * Description: Prosumer dashboard counts, including the copy saved for offline display.
 * Author: samudith
 * Created: 29/09/2026
 */
package lk.smartsolar.mobile.data.local;

import org.json.JSONObject;

public class DashboardSnapshot {
    public int pendingCount;
    public int approvedFutureCount;
    public int activeCount;
    public String nextStation = "";
    public String nextWhen = "";
    public long syncedAt;

    // Reads GET /dashboard/me and stamps the sync time.
    public static DashboardSnapshot fromJson(JSONObject json) {
        DashboardSnapshot snapshot = new DashboardSnapshot();
        snapshot.pendingCount = json.optInt("pendingCount");
        snapshot.approvedFutureCount = json.optInt("approvedFutureCount");
        snapshot.activeCount = json.optInt("activeCount");
        JSONObject next = json.optJSONObject("nextReservation");
        if (next != null) {
            snapshot.nextStation = next.optString("stationName", "");
            snapshot.nextWhen = next.optString("scheduledAt", "");
        }
        snapshot.syncedAt = System.currentTimeMillis();
        return snapshot;
    }
}
