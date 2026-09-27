/*
 * File: UserProfile.java
 * Description: Cached public profile; password data is never stored on the device.
 * Author: Dilnuk De Silva
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.local;

import org.json.JSONObject;

public class UserProfile {
    public String nic = "";
    public String username = "";
    public String fullName = "";
    public String email = "";
    public String phone = "";
    public String status = "";
    public long syncedAt;

    public static UserProfile fromJson(JSONObject json) {
        UserProfile profile = new UserProfile();
        profile.nic = json.optString("nic", json.optString("id", ""));
        profile.username = json.optString("username", "");
        profile.fullName = json.optString("fullName", "");
        profile.email = json.optString("email", "");
        profile.phone = json.optString("phone", "");
        profile.status = json.optString("status", "");
        profile.syncedAt = System.currentTimeMillis();
        return profile;
    }
}
