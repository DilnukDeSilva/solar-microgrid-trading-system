/*
 * File: BaseActivity.java
 * Description: Shared loading and API error presentation for every mobile screen.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.ui;

import android.app.Dialog;
import android.os.Bundle;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import lk.smartsolar.mobile.data.remote.ApiError;

public abstract class BaseActivity extends AppCompatActivity {
    private Dialog loading;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        ProgressBar spinner = new ProgressBar(this);
        int padding = (int) (24 * getResources().getDisplayMetrics().density);
        spinner.setPadding(padding, padding, padding, padding);
        loading = new Dialog(this);
        loading.setContentView(spinner, new ViewGroup.LayoutParams(padding * 3, padding * 3));
        loading.setCancelable(false);
    }

    protected void showLoading(boolean show) {
        if (show && !isFinishing()) loading.show(); else loading.dismiss();
    }

    protected void showError(ApiError error) {
        String message;
        if ("ACCOUNT_NOT_ACTIVE".equals(error.code)) {
            message = "Your account is awaiting Backoffice activation.";
        } else if ("NIC_EXISTS".equals(error.code)) {
            message = "An account already exists for that NIC.";
        } else {
            message = error.getMessage();
        }
        new AlertDialog.Builder(this).setTitle("Smart Solar").setMessage(message).setPositiveButton("OK", null).show();
    }

    @Override
    protected void onDestroy() {
        if (loading != null) loading.dismiss();
        super.onDestroy();
    }
}
