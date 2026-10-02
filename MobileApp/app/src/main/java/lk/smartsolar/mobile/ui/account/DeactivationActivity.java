/* File: DeactivationActivity.java | Author: DE SILVA R K D H (IT22001252) | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.account;

import android.os.Bundle;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.data.local.UserProfile;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.RoleRouter;

public class DeactivationActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state); setContentView(R.layout.activity_deactivation);
        findViewById(R.id.cancelDeactivateButton).setOnClickListener(v -> finish());
        findViewById(R.id.confirmDeactivateButton).setOnClickListener(v -> confirm());
    }

    private void confirm() {
        new MaterialAlertDialogBuilder(this).setTitle("Final confirmation")
                .setMessage("Deactivate this account and log out now?")
                .setNegativeButton("Cancel", null)
                .setPositiveButton("Deactivate", (dialog, which) -> deactivate()).show();
    }

    private void deactivate() {
        showLoading(true);
        ApiClient.get(this).requestDeactivation(new ApiCallback<UserProfile>() {
            @Override public void onSuccess(UserProfile value) {
                showLoading(false); SessionManager.get(DeactivationActivity.this).clear();
                RoleRouter.route(DeactivationActivity.this, null);
            }
            @Override public void onError(ApiError error) { showLoading(false); showError(error); }
        });
    }
}
