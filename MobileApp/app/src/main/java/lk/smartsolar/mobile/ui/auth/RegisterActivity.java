/* File: RegisterActivity.java | Author: DE SILVA R K D H (IT22001252) | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.auth;

import android.os.Bundle;
import android.widget.EditText;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.UserProfile;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;

public class RegisterActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_register);
        findViewById(R.id.createAccountButton).setOnClickListener(v -> register());
    }

    private String text(int id) { return ((EditText) findViewById(id)).getText().toString().trim(); }

    private void register() {
        String nic = text(R.id.registerNic), username = text(R.id.registerUsername), password = text(R.id.registerPassword);
        String fullName = text(R.id.registerFullName), email = text(R.id.registerEmail), phone = text(R.id.registerPhone);
        if (nic.isEmpty() || username.isEmpty() || fullName.isEmpty() || password.length() < 8) {
            showError(new ApiError(400, "VALIDATION_ERROR", "NIC, username, full name and an 8-character password are required."));
            return;
        }
        showLoading(true);
        ApiClient.get(this).register(nic, username, password, fullName, email, phone, new ApiCallback<UserProfile>() {
            @Override public void onSuccess(UserProfile value) {
                showLoading(false);
                new MaterialAlertDialogBuilder(RegisterActivity.this)
                        .setTitle("Registration received")
                        .setMessage("Your account is awaiting Backoffice activation. You can log in after it is activated.")
                        .setPositiveButton("Back to login", (dialog, which) -> finish()).setCancelable(false).show();
            }
            @Override public void onError(ApiError error) { showLoading(false); showError(error); }
        });
    }
}
