/* File: LoginActivity.java | Author: Dilnuk De Silva | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.auth;

import android.content.Intent;
import android.os.Bundle;
import android.widget.EditText;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Session;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.settings.SettingsActivity;
import lk.smartsolar.mobile.util.RoleRouter;

public class LoginActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_login);
        findViewById(R.id.loginButton).setOnClickListener(v -> login());
        findViewById(R.id.registerButton).setOnClickListener(v -> startActivity(new Intent(this, RegisterActivity.class)));
        findViewById(R.id.settingsButton).setOnClickListener(v -> startActivity(new Intent(this, SettingsActivity.class)));
    }

    private void login() {
        String identifier = ((EditText) findViewById(R.id.loginIdentifier)).getText().toString().trim();
        String password = ((EditText) findViewById(R.id.loginPassword)).getText().toString();
        if (identifier.isEmpty() || password.isEmpty()) {
            showError(new ApiError(400, "VALIDATION_ERROR", "Enter your username or NIC and password."));
            return;
        }
        showLoading(true);
        ApiClient.get(this).login(identifier, password, new ApiCallback<Session>() {
            @Override public void onSuccess(Session session) { showLoading(false); RoleRouter.route(LoginActivity.this, session); }
            @Override public void onError(ApiError error) { showLoading(false); showError(error); }
        });
    }
}
