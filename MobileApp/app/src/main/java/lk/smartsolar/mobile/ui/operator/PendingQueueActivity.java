/*
 * File: PendingQueueActivity.java
 * Description: Grid Operator approval queue. Approve calls the API and does not decide the rule itself.
 * Author: Herath D M S T (IT22639776)
 */

package lk.smartsolar.mobile.ui.operator;

import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Booking;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeText;

public class PendingQueueActivity extends BaseActivity {
    private final ArrayList<Booking> bookings = new ArrayList<>();
    private ArrayAdapter<String> adapter;

    // Loads the pending queue and approves the row the operator taps.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_pending_queue);
        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, new ArrayList<>());
        ListView list = findViewById(R.id.pendingList);
        list.setAdapter(adapter);
        list.setOnItemClickListener((parent, view, position, id) -> approve(position));
    }

    // Reloads the queue when the operator comes back to this screen.
    @Override
    protected void onResume() {
        super.onResume();
        load();
    }

    // Requests the future Pending bookings.
    private void load() {
        showLoading(true);
        ApiClient.get(this).listPendingReservations(new ApiCallback<List<Booking>>() {
            @Override
            public void onSuccess(List<Booking> result) {
                showLoading(false);
                bookings.clear();
                bookings.addAll(result);
                adapter.clear();
                if (bookings.isEmpty()) adapter.add("Nothing is waiting for approval.");
                for (Booking booking : bookings) {
                    adapter.add(booking.prosumerNic + " · " + booking.stationName + "\n" + TimeText.sriLanka(booking.scheduledAt) + "\nTap to approve");
                }
                adapter.notifyDataSetChanged();
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
            }
        });
    }

    // Approves one pending booking. The API rejects anything that is not still pending.
    private void approve(int position) {
        if (position < 0 || position >= bookings.size()) return;
        showLoading(true);
        ApiClient.get(this).approveReservation(bookings.get(position).id, new ApiCallback<Booking>() {
            @Override
            public void onSuccess(Booking value) {
                showLoading(false);
                load();
            }

            @Override
            public void onError(ApiError error) {
                showLoading(false);
                showError(error);
            }
        });
    }
}
