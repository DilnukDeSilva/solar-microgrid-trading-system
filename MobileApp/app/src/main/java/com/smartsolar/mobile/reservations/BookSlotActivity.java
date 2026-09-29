/*
 * File: BookSlotActivity.java
 * Description: Prosumer booking screen: pick a station, an optional date and a free slot, then confirm.
 *              The API checks every booking rule; this screen only shows what it returns.
 * Author: Janukshan S (IT22635266)
 */

package com.smartsolar.mobile.reservations;

import android.app.DatePickerDialog;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.smartsolar.mobile.R;
import com.smartsolar.mobile.core.ApiException;
import com.smartsolar.mobile.core.ErrorDialog;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;

public class BookSlotActivity extends AppCompatActivity {

    // Lets other screens (e.g. the map's "Book here") open this with a station already chosen.
    public static final String EXTRA_STATION_ID = "stationId";

    private ReservationApi api;
    private Spinner stationSpinner;
    private TextView dateText;
    private ListView slotList;
    private TextView emptyText;
    private ProgressBar progress;
    private Button confirmButton;

    private final List<BookingStation> stations = new ArrayList<>();
    private final List<AvailableSlot> slots = new ArrayList<>();
    private ArrayAdapter<AvailableSlot> slotAdapter;
    private String selectedDate;
    private int slotRequest;

    // Sets up the pickers and loads the stations.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_book_slot);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        }

        api = new ReservationApi(this);
        stationSpinner = findViewById(R.id.station_spinner);
        dateText = findViewById(R.id.date_text);
        slotList = findViewById(R.id.slot_list);
        emptyText = findViewById(R.id.empty_text);
        progress = findViewById(R.id.progress);
        confirmButton = findViewById(R.id.confirm_button);

        slotAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_single_choice, slots);
        slotList.setAdapter(slotAdapter);
        slotList.setOnItemClickListener((parent, view, position, id) -> confirmButton.setEnabled(true));

        findViewById(R.id.pick_date_button).setOnClickListener(v -> pickDate());
        findViewById(R.id.any_day_button).setOnClickListener(v -> setDate(null));
        confirmButton.setOnClickListener(v -> confirm());

        loadStations();
    }

    // Goes back when the toolbar arrow is pressed.
    @Override
    public boolean onSupportNavigateUp() {
        finish();
        return true;
    }

    // Loads the stations that are taking bookings.
    private void loadStations() {
        progress.setVisibility(View.VISIBLE);
        api.getBookableStations(new ReservationApi.Result<List<BookingStation>>() {
            @Override
            public void onSuccess(List<BookingStation> value) {
                progress.setVisibility(View.GONE);
                stations.clear();
                stations.addAll(value);
                showStations();
            }

            @Override
            public void onError(ApiException error) {
                progress.setVisibility(View.GONE);
                ErrorDialog.show(BookSlotActivity.this, error);
            }
        });
    }

    // Fills the station spinner and reloads slots whenever the station changes.
    private void showStations() {
        ArrayAdapter<BookingStation> adapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, stations);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        stationSpinner.setAdapter(adapter);

        String wanted = getIntent().getStringExtra(EXTRA_STATION_ID);
        for (int i = 0; i < stations.size(); i++) {
            if (stations.get(i).id.equals(wanted)) {
                stationSpinner.setSelection(i);
            }
        }

        stationSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                loadSlots();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {
            }
        });
    }

    // Opens a date picker. The min/max only guide the user; the API still decides what can be booked.
    private void pickDate() {
        Calendar today = Calendar.getInstance();
        DatePickerDialog dialog = new DatePickerDialog(this,
                (view, year, month, day) -> setDate(String.format(Locale.US, "%04d-%02d-%02d", year, month + 1, day)),
                today.get(Calendar.YEAR), today.get(Calendar.MONTH), today.get(Calendar.DAY_OF_MONTH));
        dialog.getDatePicker().setMinDate(today.getTimeInMillis());
        today.add(Calendar.DAY_OF_MONTH, 7);
        dialog.getDatePicker().setMaxDate(today.getTimeInMillis());
        dialog.show();
    }

    // Sets the date filter (null = any day) and reloads the slots.
    private void setDate(String date) {
        selectedDate = date;
        dateText.setText(date == null ? "Any day" : date);
        loadSlots();
    }

    // Loads free slots for the chosen station and date.
    private void loadSlots() {
        BookingStation station = (BookingStation) stationSpinner.getSelectedItem();
        if (station == null) {
            return;
        }

        int request = ++slotRequest;
        slots.clear();
        slotAdapter.notifyDataSetChanged();
        slotList.clearChoices();
        confirmButton.setEnabled(false);
        emptyText.setVisibility(View.GONE);
        progress.setVisibility(View.VISIBLE);

        api.getAvailableSlots(station.id, selectedDate, new ReservationApi.Result<List<AvailableSlot>>() {
            @Override
            public void onSuccess(List<AvailableSlot> value) {
                // Ignore answers to an older request if the user changed the station or date meanwhile.
                if (request != slotRequest) {
                    return;
                }
                progress.setVisibility(View.GONE);
                slots.addAll(value);
                slotAdapter.notifyDataSetChanged();
                emptyText.setVisibility(value.isEmpty() ? View.VISIBLE : View.GONE);
            }

            @Override
            public void onError(ApiException error) {
                if (request != slotRequest) {
                    return;
                }
                progress.setVisibility(View.GONE);
                ErrorDialog.show(BookSlotActivity.this, error);
            }
        });
    }

    // Asks the user to confirm the chosen slot before booking.
    private void confirm() {
        int position = slotList.getCheckedItemPosition();
        BookingStation station = (BookingStation) stationSpinner.getSelectedItem();
        if (position == ListView.INVALID_POSITION || station == null) {
            return;
        }

        AvailableSlot slot = slots.get(position);
        new MaterialAlertDialogBuilder(this)
                .setTitle("Confirm booking")
                .setMessage(station.name + "\n" + TimeFormat.full(slot.startTime) + " - " + TimeFormat.time(slot.endTime))
                .setPositiveButton("Book", (dialog, which) -> book(station, slot))
                .setNegativeButton("Back", null)
                .show();
    }

    // Sends the booking. On success the summary screen is shown.
    private void book(BookingStation station, AvailableSlot slot) {
        confirmButton.setEnabled(false);
        progress.setVisibility(View.VISIBLE);

        api.create(station.id, slot.id, new ReservationApi.Result<String>() {
            @Override
            public void onSuccess(String reservationJson) {
                progress.setVisibility(View.GONE);
                BookingSummaryActivity.open(BookSlotActivity.this, reservationJson, BookingSummaryActivity.ACTION_CREATED);
                finish();
            }

            @Override
            public void onError(ApiException error) {
                progress.setVisibility(View.GONE);
                ErrorDialog.show(BookSlotActivity.this, error);
                // The slot may have been taken meanwhile, so refresh the list.
                loadSlots();
            }
        });
    }
}
