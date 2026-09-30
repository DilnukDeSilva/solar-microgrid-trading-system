/*
 * File: StationCacheStore.java
 * Description: Offline copy of nearby stations so the map can open without signal.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */
package lk.smartsolar.mobile.data.stations;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.data.local.DbHelper;
import lk.smartsolar.mobile.data.local.SessionManager;

public class StationCacheStore {

    private final DbHelper db;

    // Uses the app's shared SQLite database.
    public StationCacheStore(Context context) {
        db = SessionManager.get(context).database();
    }

    // Replaces the cached stations with the latest nearby response.
    public void saveAll(List<NearbyStation> stations) {
        SQLiteDatabase database = db.getWritableDatabase();
        database.beginTransaction();
        try {
            long syncedAt = System.currentTimeMillis();
            database.delete("stations_cache", null, null);
            for (NearbyStation station : stations) {
                ContentValues values = new ContentValues();
                values.put("id", station.id);
                values.put("name", station.name);
                values.put("lat", station.latitude);
                values.put("lng", station.longitude);
                values.put("capacity_kwh", station.capacityKwh);
                values.put("battery_slots", station.batterySlotsTotal);
                values.put("free_slots", station.freeSlots);
                values.put("distance_km", station.distanceKm);
                values.put("synced_at", syncedAt);
                database.insert("stations_cache", null, values);
            }
            database.setTransactionSuccessful();
        } finally {
            database.endTransaction();
        }
    }

    // Returns the saved stations, nearest first.
    public List<NearbyStation> loadAll() {
        ArrayList<NearbyStation> stations = new ArrayList<>();
        try (Cursor cursor = db.getReadableDatabase().query(
                "stations_cache", null, null, null, null, null, "distance_km ASC")) {
            while (cursor.moveToNext()) {
                NearbyStation station = new NearbyStation();
                station.id = cursor.getString(cursor.getColumnIndexOrThrow("id"));
                station.name = cursor.getString(cursor.getColumnIndexOrThrow("name"));
                station.latitude = cursor.getDouble(cursor.getColumnIndexOrThrow("lat"));
                station.longitude = cursor.getDouble(cursor.getColumnIndexOrThrow("lng"));
                station.capacityKwh = cursor.getDouble(cursor.getColumnIndexOrThrow("capacity_kwh"));
                station.batterySlotsTotal = cursor.getInt(cursor.getColumnIndexOrThrow("battery_slots"));
                station.freeSlots = cursor.getInt(cursor.getColumnIndexOrThrow("free_slots"));
                station.distanceKm = cursor.getDouble(cursor.getColumnIndexOrThrow("distance_km"));
                stations.add(station);
            }
        }
        return stations;
    }

    // Milliseconds of the last successful nearby save, or 0 when the cache is empty.
    public long lastSyncedAt() {
        try (Cursor cursor = db.getReadableDatabase().rawQuery(
                "SELECT MAX(synced_at) FROM stations_cache", null)) {
            if (!cursor.moveToFirst() || cursor.isNull(0)) {
                return 0;
            }
            return cursor.getLong(0);
        }
    }
}
