/*
 * File: ProfileActivity.java
 * Description: Shows cached profile immediately, refreshes it, and saves edits through the API.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.ui.account;

import android.content.Intent;
import android.os.Bundle;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Session;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.data.local.UserProfile;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;

public class ProfileActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_profile);
        Session session = SessionManager.get(this).current();
        if (session == null || !"Prosumer".equals(session.role)) { finish(); return; }

        UserProfile cached = SessionManager.get(this).database().readProfile(session.nic);
        if (cached != null) bind(cached);
        findViewById(R.id.saveProfileButton).setOnClickListener(v -> save());
        findViewById(R.id.deactivateButton).setOnClickListener(v -> startActivity(new Intent(this, DeactivationActivity.class)));
        refresh(cached == null);
    }

    private void refresh(boolean showSpinner) {
        if (showSpinner) showLoading(true);
        ApiClient.get(this).getMyProfile(new ApiCallback<UserProfile>() {
            @Override public void onSuccess(UserProfile value) { showLoading(false); bind(value); }
            @Override public void onError(ApiError error) { showLoading(false); showError(error); }
        });
    }

    private void bind(UserProfile profile) {
        ((TextView) findViewById(R.id.profileNic)).setText("NIC: " + profile.nic);
        ((TextView) findViewById(R.id.profileStatus)).setText("Status: " + profile.status);
        ((EditText) findViewById(R.id.profileUsername)).setText(profile.username);
        ((EditText) findViewById(R.id.profileFullName)).setText(profile.fullName);
        ((EditText) findViewById(R.id.profileEmail)).setText(profile.email);
        ((EditText) findViewById(R.id.profilePhone)).setText(profile.phone);
    }

    private String text(int id) { return ((EditText) findViewById(id)).getText().toString().trim(); }

    private void save() {
        String username = text(R.id.profileUsername), fullName = text(R.id.profileFullName);
        if (username.isEmpty() || fullName.isEmpty()) {
            showError(new ApiError(400, "VALIDATION_ERROR", "Username and full name are required."));
            return;
        }
        showLoading(true);
        ApiClient.get(this).updateMyProfile(username, text(R.id.profilePassword), fullName,
                text(R.id.profileEmail), text(R.id.profilePhone), new ApiCallback<UserProfile>() {
                    @Override public void onSuccess(UserProfile value) {
                        showLoading(false); bind(value);
                        ((EditText) findViewById(R.id.profilePassword)).setText("");
                        Toast.makeText(ProfileActivity.this, "Profile updated", Toast.LENGTH_SHORT).show();
                    }
                    @Override public void onError(ApiError error) { showLoading(false); showError(error); }
                });
    }
}
