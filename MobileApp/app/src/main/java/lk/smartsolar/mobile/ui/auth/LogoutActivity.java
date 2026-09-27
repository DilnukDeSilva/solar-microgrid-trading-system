/* File: LogoutActivity.java | Author: DE SILVA R K D H (IT22001252) | Created: 27/09/2026 */
package lk.smartsolar.mobile.ui.auth;

import android.os.Bundle;

import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.RoleRouter;

public class LogoutActivity extends BaseActivity {
    @Override protected void onCreate(Bundle state) {
        super.onCreate(state);
        SessionManager.get(this).clear();
        RoleRouter.route(this, null);
    }
}
