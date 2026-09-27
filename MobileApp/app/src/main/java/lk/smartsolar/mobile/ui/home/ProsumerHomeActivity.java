/* File: ProsumerHomeActivity.java | Author: DE SILVA R K D H (IT22001252) | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.home;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.account.ProfileActivity;
import lk.smartsolar.mobile.ui.auth.LogoutActivity;

public class ProsumerHomeActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state); setContentView(R.layout.activity_home);
        ((TextView) findViewById(R.id.homeTitle)).setText("Prosumer home");
        ((TextView) findViewById(R.id.homeMessage)).setText("Your energy dashboard and booking tools will appear here.");
        findViewById(R.id.profileButton).setOnClickListener(v -> startActivity(new Intent(this, ProfileActivity.class)));
        findViewById(R.id.logoutButton).setOnClickListener(v -> startActivity(new Intent(this, LogoutActivity.class)));
    }
}
