/*
 * File: RoleRouter.java
 * Description: Central post-login routing shared by splash and login.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.util;

import android.app.Activity;
import android.content.Intent;

import lk.smartsolar.mobile.data.local.Session;
import lk.smartsolar.mobile.ui.auth.LoginActivity;
import lk.smartsolar.mobile.ui.home.OperatorHomeActivity;
import lk.smartsolar.mobile.ui.home.ProsumerHomeActivity;

public final class RoleRouter {
    private RoleRouter() { }

    public static void route(Activity source, Session session) {
        Class<?> target;
        if (session == null) target = LoginActivity.class;
        else if ("Prosumer".equals(session.role)) target = ProsumerHomeActivity.class;
        else if ("GridOperator".equals(session.role)) target = OperatorHomeActivity.class;
        else target = LoginActivity.class;
        Intent intent = new Intent(source, target).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        source.startActivity(intent);
        source.finish();
    }
}
