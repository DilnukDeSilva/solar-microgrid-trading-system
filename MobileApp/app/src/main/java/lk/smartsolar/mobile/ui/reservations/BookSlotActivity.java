/*
 * File: BookSlotActivity.java
 * Description: Prosumer booking screen: pick a station, an optional date and a free slot, then confirm.
 *              Also used to move an existing booking to another slot.
 *              The API checks every booking rule; this screen only shows what it returns.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.ui.reservations;

import android.app.DatePickerDialog;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;

import com.google.android.material.dialog.MaterialAlertDialogBuilder;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.data.reservations.AvailableSlot;
import lk.smartsolar.mobile.data.reservations.BookingStation;
import lk.smartsolar.mobile.data.reservations.Reservation;
import lk.smartsolar.mobile.data.reservations.ReservationApi;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeFormat;

public class BookSlotActivity extends BaseActivity {

    // Lets other screens (e.g. the map's "Book here") open this with a station already chosen.
    public static final String EXTRA_STATION_ID = "stationId";
    private static final String EXTRA_RESERVATION_ID = "reservationId";

    // Set when an existing booking is being moved to another slot.
    private String reservationId;

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

    // Opens the same picker to move an existing booking, starting at its current station.
    public static void openForChange(Context context, Reservation reservation) {
        Intent intent = new Intent(context, BookSlotActivity.class);
        intent.putExtra(EXTRA_RESERVATION_ID, reservation.id);
        intent.putExtra(EXTRA_STATION_ID, reservation.stationId);
        context.startActivity(intent);
    }

    // Sets up the pickers and loads the stations.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_book_slot);

        reservationId = getIntent().getStringExtra(EXTRA_RESERVATION_ID);
        if (reservationId != null) {
            ((TextView) findViewById(R.id.screen_title)).setText("Change slot");
            ((Button) findViewById(R.id.confirm_button)).setText("Save change");
        }

        api = new ReservationApi(this);
        stationSpinner = findViewById(R.id.station_spinner);
        dateText = findViewById(R.id.date_text);
        slotList = findViewById(R.id.slot_list);
        emptyText = findViewById(R.id.empty_text);
        progress = findViewById(R.id.progress);
        confirmButton = findViewById(R.id.confirm_button);

        slotAdapter = new ArrayAdapter<>(this, R.layout.item_slot, slots);
        slotList.setAdapter(slotAdapter);
        slotList.setOnItemClickListener((parent, view, position, id) -> confirmButton.setEnabled(true));

        findViewById(R.id.pick_date_button).setOnClickListener(v -> pickDate());
        findViewById(R.id.any_day_button).setOnClickListener(v -> setDate(null));
        confirmButton.setOnClickListener(v -> confirm());

        loadStations();
    }

    // Loads the stations that are taking bookings.
    private void loadStations() {
        showLoading(true);
        api.getBookableStations(new ApiCallback<List<BookingStation>>() {
            @Override
            public void onSuccess(List<BookingStation> value) {
                showLoading(false);
                stations.clear();
                stations.addAll(value);
                showStations();
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
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

        api.getAvailableSlots(station.id, selectedDate, new ApiCallback<List<AvailableSlot>>() {
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
            public void onError(ApiError error) {
                if (request != slotRequest) {
                    return;
                }
                progress.setVisibility(View.GONE);
                showError(error);
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
        boolean changing = reservationId != null;
        new MaterialAlertDialogBuilder(this)
                .setTitle(changing ? "Move booking to this slot?" : "Confirm booking")
                .setMessage(station.name + "\n" + TimeFormat.full(slot.startTime) + " - " + TimeFormat.time(slot.endTime)
                        + (changing ? "\n\nAn approved booking needs to be approved again after a change." : ""))
                .setPositiveButton(changing ? "Move" : "Book", (dialog, which) -> book(station, slot))
                .setNegativeButton("Back", null)
                .show();
    }

    // Sends the new booking, or the change for an existing one. On success the summary screen is shown.
    private void book(BookingStation station, AvailableSlot slot) {
        confirmButton.setEnabled(false);
        showLoading(true);

        boolean changing = reservationId != null;
        ApiCallback<String> callback = new ApiCallback<String>() {
            @Override
            public void onSuccess(String reservationJson) {
                showLoading(false);
                BookingSummaryActivity.open(BookSlotActivity.this, reservationJson,
                        changing ? BookingSummaryActivity.ACTION_UPDATED : BookingSummaryActivity.ACTION_CREATED);
                finish();
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
                // The slot may have been taken meanwhile, so refresh the list.
                loadSlots();
            }
        };

        if (changing) {
            api.update(reservationId, station.id, slot.id, callback);
        } else {
            api.create(station.id, slot.id, callback);
        }
    }
}
