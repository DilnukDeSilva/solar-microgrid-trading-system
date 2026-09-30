/*
 * File: StationListActivity.java
 * Description: Nearby stations as a list. A row opens the same booking screen as the map.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */
package lk.smartsolar.mobile.ui.stations;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.core.content.ContextCompat;

import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.model.LatLng;

import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiError;
import lk.smartsolar.mobile.data.stations.NearbyStation;
import lk.smartsolar.mobile.data.stations.StationApi;
import lk.smartsolar.mobile.data.stations.StationCacheStore;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.reservations.BookSlotActivity;
import lk.smartsolar.mobile.util.TimeText;

public class StationListActivity extends BaseActivity {

    private static final LatLng COLOMBO = new LatLng(6.9271, 79.8612);

    private StationCacheStore cache;
    private StationApi api;
    private TextView offlineLabel;
    private ArrayAdapter<NearbyStation> adapter;
    private ActivityResultLauncher<String[]> locationPermission;

    // Shows the saved list, then refreshes it from the phone's location.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_station_list);
        cache = new StationCacheStore(this);
        api = new StationApi(this);
        offlineLabel = findViewById(R.id.offlineLabel);
        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, new ArrayList<>());
        ListView list = findViewById(R.id.stationList);
        list.setAdapter(adapter);
        list.setOnItemClickListener((parent, view, position, id) -> {
            NearbyStation station = adapter.getItem(position);
            if (station == null) return;
            startActivity(new Intent(this, BookSlotActivity.class)
                    .putExtra(BookSlotActivity.EXTRA_STATION_ID, station.id));
        });
        show(cache.loadAll(), cache.lastSyncedAt() > 0);
        locationPermission = registerForActivityResult(
                new ActivityResultContracts.RequestMultiplePermissions(),
                result -> {
                    boolean granted = Boolean.TRUE.equals(result.get(Manifest.permission.ACCESS_FINE_LOCATION))
                            || Boolean.TRUE.equals(result.get(Manifest.permission.ACCESS_COARSE_LOCATION));
                    if (granted) {
                        useDeviceLocation();
                    } else {
                        loadAround(COLOMBO);
                    }
                });
        if (hasLocationPermission()) {
            useDeviceLocation();
        } else {
            locationPermission.launch(new String[]{
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION
            });
        }
    }

    // True when the user already allowed fine or coarse location.
    private boolean hasLocationPermission() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED
                || ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_COARSE_LOCATION) == PackageManager.PERMISSION_GRANTED;
    }

    // Loads stations around the last known point, or Colombo when location is missing.
    private void useDeviceLocation() {
        if (!hasLocationPermission()) {
            loadAround(COLOMBO);
            return;
        }
        LocationServices.getFusedLocationProviderClient(this).getLastLocation()
                .addOnSuccessListener(this, location -> {
                    if (location == null) {
                        loadAround(COLOMBO);
                    } else {
                        loadAround(new LatLng(location.getLatitude(), location.getLongitude()));
                    }
                })
                .addOnFailureListener(this, error -> loadAround(COLOMBO));
    }

    // Asks the API for stations near this point and saves them for the map.
    private void loadAround(LatLng point) {
        api.getNearby(point.latitude, point.longitude, new ApiCallback<List<NearbyStation>>() {
            @Override
            public void onSuccess(List<NearbyStation> stations) {
                cache.saveAll(stations);
                show(stations, false);
            }

            @Override
            public void onError(ApiError error) {
                show(cache.loadAll(), true);
            }
        });
    }

    // Paints the rows. Offline keeps the last saved list and the banner.
    private void show(List<NearbyStation> stations, boolean offline) {
        adapter.clear();
        adapter.addAll(stations);
        if (!offline) {
            offlineLabel.setVisibility(View.GONE);
            return;
        }
        long syncedAt = cache.lastSyncedAt();
        if (syncedAt <= 0) {
            offlineLabel.setText("Showing saved stations");
        } else {
            String when = TimeText.lastUpdated(syncedAt);
            offlineLabel.setText("Showing saved stations, " + Character.toLowerCase(when.charAt(0)) + when.substring(1));
        }
        offlineLabel.setVisibility(View.VISIBLE);
    }
}
