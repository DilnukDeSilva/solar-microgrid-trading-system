/*
 * File: BookingSummaryActivity.java
 * Description: Summary shown after a booking is created, updated or cancelled: station, slot time, status and reference.
 * Author: Janukshan S (IT22635266)
 */

package com.smartsolar.mobile.reservations;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.activity.OnBackPressedCallback;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;

import com.smartsolar.mobile.R;
import com.smartsolar.mobile.core.HomeActivity;

import org.json.JSONException;
import org.json.JSONObject;

public class BookingSummaryActivity extends AppCompatActivity {

    public static final String ACTION_CREATED = "created";
    public static final String ACTION_UPDATED = "updated";
    public static final String ACTION_CANCELLED = "cancelled";

    private static final String EXTRA_RESERVATION = "reservation";
    private static final String EXTRA_ACTION = "action";

    // Opens the summary for the reservation the API just returned.
    public static void open(Context context, String reservationJson, String action) {
        Intent intent = new Intent(context, BookingSummaryActivity.class);
        intent.putExtra(EXTRA_RESERVATION, reservationJson);
        intent.putExtra(EXTRA_ACTION, action);
        context.startActivity(intent);
    }

    // Fills the header and the detail rows from the reservation.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_booking_summary);

        Reservation reservation;
        try {
            reservation = Reservation.fromJson(new JSONObject(getIntent().getStringExtra(EXTRA_RESERVATION)));
        } catch (JSONException | NullPointerException e) {
            finish();
            return;
        }

        showHeader(getIntent().getStringExtra(EXTRA_ACTION));

        LinearLayout details = findViewById(R.id.details);
        addRow(details, "Station", reservation.stationName);
        addRow(details, "Slot time", TimeFormat.full(reservation.scheduledAt));
        TextView status = addRow(details, "Status", reservation.status);
        status.setTextColor(ContextCompat.getColor(this, StatusStyle.colour(reservation.status)));
        addRow(details, "Reference", reservation.id);

        findViewById(R.id.home_button).setOnClickListener(v -> goHome());
        getOnBackPressedDispatcher().addCallback(this, new OnBackPressedCallback(true) {
            @Override
            public void handleOnBackPressed() {
                goHome();
            }
        });
    }

    // Sets the icon, title and colour for what just happened.
    private void showHeader(String action) {
        View header = findViewById(R.id.header);
        TextView icon = findViewById(R.id.header_icon);
        TextView title = findViewById(R.id.header_title);
        TextView message = findViewById(R.id.header_message);

        if (ACTION_CANCELLED.equals(action)) {
            icon.setText("✕");
            title.setText("Booking cancelled");
            message.setText("The slot has been released.");
            header.setBackgroundColor(ContextCompat.getColor(this, R.color.status_cancelled));
        } else if (ACTION_UPDATED.equals(action)) {
            icon.setText("↻");
            title.setText("Booking updated");
            message.setText("Your booking needs to be approved again before the visit.");
            header.setBackgroundColor(ContextCompat.getColor(this, R.color.status_completed));
        } else {
            icon.setText("✓");
            title.setText("Booking created");
            message.setText("A grid operator will approve it. Your QR code appears once it is approved.");
            header.setBackgroundColor(ContextCompat.getColor(this, R.color.solar_green));
        }
    }

    // Adds a "label: value" row to the details list and returns the value view.
    private TextView addRow(LinearLayout parent, String label, String value) {
        View row = LayoutInflater.from(this).inflate(R.layout.item_summary_row, parent, false);
        ((TextView) row.findViewById(R.id.label)).setText(label);
        TextView valueView = row.findViewById(R.id.value);
        valueView.setText(value);
        parent.addView(row);
        return valueView;
    }

    // Returns to the home screen without stacking another copy of it.
    private void goHome() {
        Intent intent = new Intent(this, HomeActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        startActivity(intent);
        finish();
    }
}
