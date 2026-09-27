/* File: SplashActivity.java | Author: Dilnuk De Silva | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.auth;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.RoleRouter;

public class SplashActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_splash);
        new Handler(Looper.getMainLooper()).postDelayed(
                () -> RoleRouter.route(this, SessionManager.get(this).current()), 700);
    }
}
