/*
 * File: Session.java
 * Description: Keeps the logged-in user's JWT, role and NIC. Stand-in for Member 1's app shell,
 *              written by Janukshan S (IT22635266) so the booking screens can run before the shell is merged.
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import android.content.Context;
import android.content.SharedPreferences;

import org.json.JSONObject;

public class Session {

    private final SharedPreferences prefs;

    // Opens the private preferences file that holds the session.
    public Session(Context context) {
        prefs = context.getApplicationContext().getSharedPreferences("session", Context.MODE_PRIVATE);
    }

    // Saves the token and user details from the login response.
    public void save(JSONObject login) {
        JSONObject user = login.optJSONObject("user");
        prefs.edit()
                .putString("token", login.optString("token"))
                .putString("role", user == null ? "" : user.optString("role"))
                .putString("nic", user == null ? "" : user.optString("nic"))
                .putString("fullName", user == null ? "" : user.optString("fullName"))
                .apply();
    }

    // Removes everything, used on logout or when the token is rejected.
    public void clear() {
        prefs.edit().clear().apply();
    }

    public boolean isLoggedIn() {
        return getToken() != null;
    }

    public String getToken() {
        String token = prefs.getString("token", "");
        return token.isEmpty() ? null : token;
    }

    public String getRole() {
        return prefs.getString("role", "");
    }

    public String getFullName() {
        return prefs.getString("fullName", "");
    }
}
