/*
 * File: QrDetailsActivity.java
 * Description: Shows a verified booking and sends Finalise to the API.
 * Author: Herath D M S T (IT22639776)
 */

package lk.smartsolar.mobile.ui.operator;

import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Booking;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeText;

public class QrDetailsActivity extends BaseActivity {
    private String reservationId;

    // Shows the booking returned by verify-qr.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_qr_details);
        reservationId = getIntent().getStringExtra("id");
        String status = getIntent().getStringExtra("status");
        ((TextView) findViewById(R.id.resultTitle)).setText("Approved".equals(status) ? "QR verified" : "Booking");
        ((TextView) findViewById(R.id.resultBody)).setText(
                "Station: " + getIntent().getStringExtra("station")
                        + "\nNIC: " + getIntent().getStringExtra("nic")
                        + "\nWhen: " + TimeText.sriLanka(getIntent().getStringExtra("when"))
                        + "\nStatus: " + status);
        View finalise = findViewById(R.id.finaliseButton);
        finalise.setVisibility("Approved".equals(status) ? View.VISIBLE : View.GONE);
        finalise.setOnClickListener(v -> complete());
    }

    // Marks the job completed. A second attempt is rejected by the API.
    private void complete() {
        showLoading(true);
        ApiClient.get(this).completeReservation(reservationId, new ApiCallback<Booking>() {
            @Override
            public void onSuccess(Booking booking) {
                showLoading(false);
                ((TextView) findViewById(R.id.resultTitle)).setText("Job completed");
                ((TextView) findViewById(R.id.resultBody)).setText(
                        booking.stationName + "\n" + booking.prosumerNic + "\nStatus: " + booking.status);
                findViewById(R.id.finaliseButton).setVisibility(View.GONE);
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
            }
        });
    }
}
