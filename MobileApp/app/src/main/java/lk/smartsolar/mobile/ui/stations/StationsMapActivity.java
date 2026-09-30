/*
 * File: StationsMapActivity.java
 * Description: Nearby stations on a Google Map. Cached pins show first; a tap opens booking.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */
package lk.smartsolar.mobile.ui.stations;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.core.content.ContextCompat;

import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.material.bottomsheet.BottomSheetDialog;

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

public class StationsMapActivity extends BaseActivity implements OnMapReadyCallback {

    private static final LatLng COLOMBO = new LatLng(6.9271, 79.8612);

    private GoogleMap map;
    private StationCacheStore cache;
    private StationApi api;
    private TextView offlineLabel;
    private ActivityResultLauncher<String[]> locationPermission;

    // Shows the map, then asks Google Maps to call back when the tiles are ready.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_stations_map);
        cache = new StationCacheStore(this);
        api = new StationApi(this);
        offlineLabel = findViewById(R.id.offlineLabel);
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
        SupportMapFragment fragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        fragment.getMapAsync(this);
    }

    // Draws the saved pins first, then asks for the phone's location.
    @Override
    public void onMapReady(GoogleMap googleMap) {
        map = googleMap;
        map.setOnMarkerClickListener(marker -> {
            Object tag = marker.getTag();
            if (tag instanceof NearbyStation) {
                showStation((NearbyStation) tag);
            }
            return true;
        });
        draw(cache.loadAll());
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

    // Turns on the blue dot and loads stations around the last known point.
    private void useDeviceLocation() {
        if (!hasLocationPermission()) {
            loadAround(COLOMBO);
            return;
        }
        try {
            map.setMyLocationEnabled(true);
        } catch (SecurityException ignored) {
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

    // Moves the camera, then replaces the pins when the nearby call succeeds.
    private void loadAround(LatLng point) {
        map.moveCamera(CameraUpdateFactory.newLatLngZoom(point, 12));
        api.getNearby(point.latitude, point.longitude, new ApiCallback<List<NearbyStation>>() {
            @Override
            public void onSuccess(List<NearbyStation> stations) {
                cache.saveAll(stations);
                draw(stations);
                offlineLabel.setVisibility(View.GONE);
            }

            @Override
            public void onError(ApiError error) {
                showOffline();
            }
        });
    }

    // Clears the map and drops one pin per station.
    private void draw(List<NearbyStation> stations) {
        map.clear();
        for (NearbyStation station : stations) {
            Marker marker = map.addMarker(new MarkerOptions()
                    .position(new LatLng(station.latitude, station.longitude))
                    .title(station.name));
            if (marker != null) {
                marker.setTag(station);
            }
        }
    }

    // Shows the cached-data banner with the time of the last successful save.
    private void showOffline() {
        long syncedAt = cache.lastSyncedAt();
        if (syncedAt <= 0) {
            offlineLabel.setText("Showing saved stations");
        } else {
            String when = TimeText.lastUpdated(syncedAt);
            offlineLabel.setText("Showing saved stations, " + Character.toLowerCase(when.charAt(0)) + when.substring(1));
        }
        offlineLabel.setVisibility(View.VISIBLE);
    }

    // Opens the station details and a button into Janukshan's booking screen.
    private void showStation(NearbyStation station) {
        BottomSheetDialog sheet = new BottomSheetDialog(this);
        View view = getLayoutInflater().inflate(R.layout.bottom_sheet_station, null);
        ((TextView) view.findViewById(R.id.stationName)).setText(station.name);
        ((TextView) view.findViewById(R.id.stationDistance)).setText(station.distanceKm + " km away");
        ((TextView) view.findViewById(R.id.stationCapacity)).setText("Capacity: " + station.capacityKwh + " kWh");
        ((TextView) view.findViewById(R.id.stationSlots)).setText("Battery slots: " + station.batterySlotsTotal);
        ((TextView) view.findViewById(R.id.stationFreeSlots)).setText("Free slots: " + station.freeSlots);
        view.findViewById(R.id.bookHereButton).setOnClickListener(v -> {
            sheet.dismiss();
            startActivity(new Intent(this, BookSlotActivity.class)
                    .putExtra(BookSlotActivity.EXTRA_STATION_ID, station.id));
        });
        sheet.setContentView(view);
        sheet.show();
    }
}
