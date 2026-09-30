/* File: ProsumerHomeActivity.java | Author: samudith | Created: 29/09/2026 */
package lk.smartsolar.mobile.ui.home;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;

import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.DashboardSnapshot;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.account.ProfileActivity;
import lk.smartsolar.mobile.ui.auth.LogoutActivity;
import lk.smartsolar.mobile.ui.bookings.BookingsActivity;
import lk.smartsolar.mobile.ui.reservations.BookSlotActivity;
import lk.smartsolar.mobile.ui.stations.StationsMapActivity;
import lk.smartsolar.mobile.util.TimeText;

public class ProsumerHomeActivity extends BaseActivity {
    private SwipeRefreshLayout refresh;

    // Loads the live dashboard, falling back to the last SQLite snapshot.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_prosumer_dashboard);
        refresh = findViewById(R.id.refresh);
        refresh.setOnRefreshListener(this::loadDashboard);
        findViewById(R.id.bookSlotButton).setOnClickListener(v -> startActivity(new Intent(this, BookSlotActivity.class)));
        findViewById(R.id.nearbyStationsButton).setOnClickListener(v -> startActivity(new Intent(this, StationsMapActivity.class)));
        findViewById(R.id.bookingsButton).setOnClickListener(v -> startActivity(new Intent(this, BookingsActivity.class)));
        findViewById(R.id.profileButton).setOnClickListener(v -> startActivity(new Intent(this, ProfileActivity.class)));
        findViewById(R.id.logoutButton).setOnClickListener(v -> startActivity(new Intent(this, LogoutActivity.class)));
        showSnapshot(SessionManager.get(this).database().readDashboard(), true);
        loadDashboard();
    }

    // Requests GET /dashboard/me and paints the counts.
    private void loadDashboard() {
        refresh.setRefreshing(true);
        ApiClient.get(this).getMyDashboard(new lk.smartsolar.mobile.data.remote.ApiCallback<DashboardSnapshot>() {
            @Override
            public void onSuccess(DashboardSnapshot snapshot) {
                refresh.setRefreshing(false);
                showSnapshot(snapshot, false);
            }

            @Override
            public void onError(lk.smartsolar.mobile.data.remote.ApiError error) {
                refresh.setRefreshing(false);
                DashboardSnapshot cached = SessionManager.get(ProsumerHomeActivity.this).database().readDashboard();
                showSnapshot(cached, true);
                if (cached == null) showError(error);
            }
        });
    }

    // Writes the counts onto the screen and labels cached data.
    private void showSnapshot(DashboardSnapshot snapshot, boolean offline) {
        TextView updated = findViewById(R.id.updatedLabel);
        TextView pending = findViewById(R.id.pendingCount);
        TextView approved = findViewById(R.id.approvedCount);
        TextView active = findViewById(R.id.activeCount);
        TextView next = findViewById(R.id.nextBooking);
        if (snapshot == null) {
            updated.setText("No saved dashboard yet");
            pending.setText("Pending: —");
            approved.setText("Approved upcoming: —");
            active.setText("Active: —");
            next.setText("Next booking: none");
            return;
        }

        updated.setText((offline ? "Offline. " : "") + TimeText.lastUpdated(snapshot.syncedAt));
        pending.setText("Pending: " + snapshot.pendingCount);
        approved.setText("Approved upcoming: " + snapshot.approvedFutureCount);
        active.setText("Active: " + snapshot.activeCount);
        if (snapshot.nextStation == null || snapshot.nextStation.isEmpty()) {
            next.setText("Next booking: none");
        } else {
            next.setText("Next booking: " + snapshot.nextStation + "\n" + TimeText.sriLanka(snapshot.nextWhen));
        }
    }
}
