/*
 * File: StatusStyle.java
 * Description: Colour used for each reservation status (same colours as the web badges), and a rounded status chip.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.ui.reservations;

import android.graphics.drawable.GradientDrawable;
import android.widget.TextView;

import androidx.core.content.ContextCompat;

import lk.smartsolar.mobile.R;

public final class StatusStyle {

    private StatusStyle() {
    }

    // Returns the colour resource for a status.
    public static int colour(String status) {
        if ("Pending".equals(status)) {
            return R.color.status_pending;
        }
        if ("Approved".equals(status)) {
            return R.color.status_approved;
        }
        if ("Completed".equals(status)) {
            return R.color.status_completed;
        }
        return R.color.status_cancelled;
    }

    // Turns a TextView into a small coloured pill showing the status.
    public static void chip(TextView view, String status) {
        float density = view.getResources().getDisplayMetrics().density;
        GradientDrawable background = new GradientDrawable();
        background.setColor(ContextCompat.getColor(view.getContext(), colour(status)));
        background.setCornerRadius(50 * density);
        view.setBackground(background);
        view.setTextColor(ContextCompat.getColor(view.getContext(), R.color.white));
        int horizontal = (int) (12 * density);
        int vertical = (int) (4 * density);
        view.setPadding(horizontal, vertical, horizontal, vertical);
        view.setText(status);
    }
}
