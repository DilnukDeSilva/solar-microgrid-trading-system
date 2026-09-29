/*
 * File: BookingsActivity.java
 * Description: Current, pending and history tabs for the signed-in prosumer, with a keyword filter.
 * Author: samudith
 * Created: 29/09/2026
 */
package lk.smartsolar.mobile.ui.bookings;

import android.os.Bundle;
import android.view.inputmethod.EditorInfo;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

import java.time.Instant;
import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Booking;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeText;

public class BookingsActivity extends BaseActivity {
    private String tab = "current";
    private ArrayAdapter<String> adapter;

    // Wires the three tabs and the keyword search, then loads Current.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_bookings);
        ((TextView) findViewById(R.id.screenTitle)).setText("My bookings");
        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, new ArrayList<>());
        ((ListView) findViewById(R.id.bookingList)).setAdapter(adapter);
        findViewById(R.id.currentTab).setOnClickListener(v -> { tab = "current"; load(); });
        findViewById(R.id.pendingTab).setOnClickListener(v -> { tab = "pending"; load(); });
        findViewById(R.id.historyTab).setOnClickListener(v -> { tab = "history"; load(); });
        ((EditText) findViewById(R.id.searchBox)).setOnEditorActionListener((view, actionId, event) -> {
            if (actionId == EditorInfo.IME_ACTION_SEARCH) {
                load();
                return true;
            }
            return false;
        });
        load();
    }

    // Asks the API for this tab. On failure it shows the SQLite copy.
    private void load() {
        showLoading(true);
        ApiClient.get(this).listReservations(query(), new ApiCallback<List<Booking>>() {
            @Override
            public void onSuccess(List<Booking> bookings) {
                showLoading(false);
                showRows(bookings, false);
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                List<Booking> cached = SessionManager.get(BookingsActivity.this).database().readBookings();
                showRows(cached, true);
                if (cached.isEmpty()) showError(error);
            }
        });
    }

    // Builds the filter query. The prosumer NIC is applied by the API from the token.
    private String query() {
        StringBuilder query = new StringBuilder("page=1");
        String keyword = ((EditText) findViewById(R.id.searchBox)).getText().toString().trim();
        if (!keyword.isEmpty()) query.append("&q=").append(android.net.Uri.encode(keyword));
        String now = Instant.now().toString();
        if ("current".equals(tab)) query.append("&from=").append(android.net.Uri.encode(now));
        if ("pending".equals(tab)) query.append("&status=Pending");
        if ("history".equals(tab)) query.append("&to=").append(android.net.Uri.encode(now));
        return query.toString();
    }

    // Renders one line per booking and the offline label when the list is cached.
    private void showRows(List<Booking> bookings, boolean offline) {
        TextView updated = findViewById(R.id.updatedLabel);
        long syncedAt = bookings.isEmpty() ? 0 : bookings.get(0).syncedAt;
        updated.setText(offline ? "Offline. " + TimeText.lastUpdated(syncedAt) : "");
        adapter.clear();
        if (bookings.isEmpty()) {
            adapter.add("No bookings in this tab.");
        }
        for (Booking booking : bookings) {
            adapter.add(booking.status + " · " + booking.stationName + "\n" + TimeText.sriLanka(booking.scheduledAt));
        }
        adapter.notifyDataSetChanged();
    }
}
