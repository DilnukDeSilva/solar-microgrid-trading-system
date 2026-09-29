/*
 * File: ErrorDialog.java
 * Description: Shows the API's error message. A 401 means the login expired, so it goes back to login.
 *              Stand-in for Member 1's app shell, written by Janukshan S (IT22635266).
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import android.app.Activity;
import android.content.Intent;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;

public final class ErrorDialog {

    private ErrorDialog() {
    }

    // Shows the server's message, or signs out on 401.
    public static void show(Activity activity, ApiException error) {
        if (activity.isFinishing()) {
            return;
        }

        if (error.getStatus() == 401) {
            new Session(activity).clear();
            Intent intent = new Intent(activity, LoginActivity.class);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            activity.startActivity(intent);
            return;
        }

        new MaterialAlertDialogBuilder(activity)
                .setTitle("Something went wrong")
                .setMessage(error.getMessage())
                .setPositiveButton("OK", null)
                .show();
    }
}
