/*
 * File: StatusStyle.java
 * Description: Colour used for each reservation status, the same colours as the web badges.
 * Author: Janukshan S (IT22635266)
 */

package com.smartsolar.mobile.reservations;

import com.smartsolar.mobile.R;

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
}
