/* File: SettingsActivity.java | Author: Dilnuk De Silva | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.settings;

import android.content.Context;
import android.os.Bundle;
import android.widget.EditText;
import android.widget.Toast;

import lk.smartsolar.mobile.BuildConfig;
import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;

public class SettingsActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state); setContentView(R.layout.activity_settings);
        EditText field = findViewById(R.id.apiBaseUrl);
        field.setText(getSharedPreferences("settings", Context.MODE_PRIVATE).getString("api_base_url", BuildConfig.API_BASE_URL));
        findViewById(R.id.saveSettingsButton).setOnClickListener(v -> {
            String url = field.getText().toString().trim();
            if (!url.startsWith("http://") && !url.startsWith("https://")) {
                showError(new ApiError(400, "VALIDATION_ERROR", "Enter a complete http:// or https:// API URL."));
                return;
            }
            if (!url.endsWith("/")) url += "/";
            getSharedPreferences("settings", Context.MODE_PRIVATE).edit().putString("api_base_url", url).apply();
            Toast.makeText(this, "API address saved", Toast.LENGTH_SHORT).show();
            finish();
        });
    }
}
