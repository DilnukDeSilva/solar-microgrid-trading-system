/* File: OperatorHomeActivity.java | Author: Dilnuk De Silva | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.home;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.auth.LogoutActivity;

public class OperatorHomeActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state); setContentView(R.layout.activity_home);
        ((TextView) findViewById(R.id.homeTitle)).setText("Grid operator home");
        ((TextView) findViewById(R.id.homeMessage)).setText("Operator tools will appear here.");
        findViewById(R.id.profileButton).setVisibility(View.GONE);
        findViewById(R.id.logoutButton).setOnClickListener(v -> startActivity(new Intent(this, LogoutActivity.class)));
    }
}
