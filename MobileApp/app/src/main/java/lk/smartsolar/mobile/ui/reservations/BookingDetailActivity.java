/*
 * File: BookingDetailActivity.java
 * Description: One of the prosumer's bookings with Change slot, Cancel and Show QR. Loads from the API and
 *              falls back to the SQLite copy when there is no signal. The API decides whether a change is allowed.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.ui.reservations;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;

import org.json.JSONObject;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.data.reservations.MyReservationStore;
import lk.smartsolar.mobile.data.reservations.Reservation;
import lk.smartsolar.mobile.data.reservations.ReservationApi;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeFormat;
import lk.smartsolar.mobile.util.TimeText;

public class BookingDetailActivity extends BaseActivity {

    private static final String EXTRA_ID = "reservationId";

    private ReservationApi api;
    private String reservationId;
    private Reservation reservation;

    // Opens the booking screen for one reservation.
    public static void open(Context context, String reservationId) {
        Intent intent = new Intent(context, BookingDetailActivity.class);
        intent.putExtra(EXTRA_ID, reservationId);
        context.startActivity(intent);
    }

    // Shows the saved copy straight away, then refreshes from the API.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_booking_detail);

        api = new ReservationApi(this);
        reservationId = getIntent().getStringExtra(EXTRA_ID);

        findViewById(R.id.qr_button).setOnClickListener(v -> BookingQrActivity.open(this, reservationId));
        findViewById(R.id.change_button).setOnClickListener(v -> BookSlotActivity.openForChange(this, reservation));
        findViewById(R.id.cancel_button).setOnClickListener(v -> confirmCancel());

        Reservation saved = new MyReservationStore(this).find(reservationId);
        if (saved != null) {
            show(saved, false);
        }
    }

    // Reloads every time the screen comes back, so a change or cancel made elsewhere is shown.
    @Override
    protected void onResume() {
        super.onResume();
        load();
    }

    // Loads the latest copy from the API. Without signal, the saved copy stays on screen.
    private void load() {
        showLoading(reservation == null);
        api.get(reservationId, new ApiCallback<String>() {
            @Override
            public void onSuccess(String json) {
                showLoading(false);
                try {
                    show(Reservation.fromJson(new JSONObject(json)), false);
                } catch (Exception ignored) {
                }
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                if (reservation != null && error.status == 0) {
                    show(reservation, true);
                } else {
                    showError(error);
                }
            }
        });
    }

    // Fills the card and shows the buttons that make sense for the booking's status.
    private void show(Reservation r, boolean offline) {
        reservation = r;

        TextView offlineLabel = findViewById(R.id.offline_label);
        offlineLabel.setVisibility(offline ? View.VISIBLE : View.GONE);
        offlineLabel.setText("Offline. Showing the copy saved on this phone. " + TimeText.lastUpdated(r.syncedAt));

        ((TextView) findViewById(R.id.station_name)).setText(r.stationName);
        ((TextView) findViewById(R.id.slot_time)).setText(TimeFormat.full(r.scheduledAt));
        StatusStyle.chip(findViewById(R.id.status_chip), r.status);
        ((TextView) findViewById(R.id.reference_text)).setText("Reference: " + r.id);

        boolean open = "Pending".equals(r.status) || "Approved".equals(r.status);
        boolean hasQr = "Approved".equals(r.status) && r.qrToken != null && !r.qrToken.isEmpty();

        TextView hint = findViewById(R.id.hint_text);
        if ("Pending".equals(r.status)) {
            hint.setText("Waiting for a grid operator to approve it. The QR code appears once it is approved.");
        } else if (hasQr) {
            hint.setText("Approved. Show the QR code to the grid operator at the station.");
        } else {
            hint.setText("This booking is " + r.status + " and can no longer be changed.");
        }

        // Buttons are hidden offline because changes need the API.
        findViewById(R.id.qr_button).setVisibility(hasQr ? View.VISIBLE : View.GONE);
        findViewById(R.id.change_button).setVisibility(open && !offline ? View.VISIBLE : View.GONE);
        findViewById(R.id.cancel_button).setVisibility(open && !offline ? View.VISIBLE : View.GONE);
    }

    // Asks before cancelling. The 12-hour rule is checked by the API, not here.
    private void confirmCancel() {
        new MaterialAlertDialogBuilder(this)
                .setTitle("Cancel this booking?")
                .setMessage(reservation.stationName + "\n" + TimeFormat.full(reservation.scheduledAt)
                        + "\n\nBookings can only be cancelled at least 12 hours before the slot starts.")
                .setPositiveButton("Cancel booking", (dialog, which) -> cancel())
                .setNegativeButton("Keep it", null)
                .show();
    }

    // Sends the cancellation and shows the summary screen.
    private void cancel() {
        showLoading(true);
        api.cancel(reservationId, new ApiCallback<String>() {
            @Override
            public void onSuccess(String json) {
                showLoading(false);
                BookingSummaryActivity.open(BookingDetailActivity.this, json, BookingSummaryActivity.ACTION_CANCELLED);
                finish();
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
            }
        });
    }
}
