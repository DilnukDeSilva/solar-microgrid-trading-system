/*
 * File: DbHelper.java
 * Description: Single SQLite database for sessions and module-owned offline tables.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.local;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

import java.util.ArrayList;
import java.util.List;

public class DbHelper extends SQLiteOpenHelper {
    private static final String DB_NAME = "smart_solar.db";
    // Every module adds its CREATE statement and increments this version.
    private static final int DB_VERSION = 2;

    public DbHelper(Context context) {
        super(context.getApplicationContext(), DB_NAME, null, DB_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL("CREATE TABLE session (token TEXT NOT NULL, expires_at INTEGER NOT NULL, role TEXT NOT NULL, user_id TEXT NOT NULL, nic TEXT)");
        db.execSQL("CREATE TABLE user_profile (nic TEXT PRIMARY KEY, username TEXT NOT NULL, full_name TEXT NOT NULL, email TEXT NOT NULL, phone TEXT NOT NULL, status TEXT NOT NULL, synced_at INTEGER NOT NULL)");
        createBookingCache(db);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        // Additive migrations belong here. Never drop other modules' offline data.
        if (oldVersion < 2) {
            createBookingCache(db);
        }
    }

    // Creates the offline booking list and dashboard snapshot tables.
    private void createBookingCache(SQLiteDatabase db) {
        db.execSQL("CREATE TABLE IF NOT EXISTS booking_history_cache (id TEXT PRIMARY KEY, prosumer_nic TEXT, station_name TEXT, scheduled_at TEXT, status TEXT, synced_at INTEGER NOT NULL)");
        db.execSQL("CREATE TABLE IF NOT EXISTS dashboard_snapshot (id INTEGER PRIMARY KEY CHECK (id = 1), pending_count INTEGER NOT NULL, approved_future_count INTEGER NOT NULL, active_count INTEGER NOT NULL, next_station TEXT, next_when TEXT, synced_at INTEGER NOT NULL)");
    }

    public synchronized void saveSession(Session session) {
        SQLiteDatabase db = getWritableDatabase();
        db.beginTransaction();
        try {
            db.delete("session", null, null);
            ContentValues values = new ContentValues();
            values.put("token", session.token);
            values.put("expires_at", session.expiresAt);
            values.put("role", session.role);
            values.put("user_id", session.userId);
            values.put("nic", session.nic);
            db.insertOrThrow("session", null, values);
            db.setTransactionSuccessful();
        } finally {
            db.endTransaction();
        }
    }

    public synchronized Session readSession() {
        try (Cursor cursor = getReadableDatabase().query("session", null, null, null, null, null, null, "1")) {
            if (!cursor.moveToFirst()) return null;
            return new Session(
                    cursor.getString(cursor.getColumnIndexOrThrow("token")),
                    cursor.getLong(cursor.getColumnIndexOrThrow("expires_at")),
                    cursor.getString(cursor.getColumnIndexOrThrow("role")),
                    cursor.getString(cursor.getColumnIndexOrThrow("user_id")),
                    cursor.getString(cursor.getColumnIndexOrThrow("nic")));
        }
    }

    public synchronized void clearSession() {
        getWritableDatabase().delete("session", null, null);
    }

    public synchronized void saveProfile(UserProfile profile) {
        ContentValues values = new ContentValues();
        values.put("nic", profile.nic);
        values.put("username", profile.username);
        values.put("full_name", profile.fullName);
        values.put("email", profile.email);
        values.put("phone", profile.phone);
        values.put("status", profile.status);
        values.put("synced_at", profile.syncedAt);
        getWritableDatabase().insertWithOnConflict("user_profile", null, values, SQLiteDatabase.CONFLICT_REPLACE);
    }

    public synchronized UserProfile readProfile(String nic) {
        try (Cursor cursor = getReadableDatabase().query("user_profile", null, "nic = ?", new String[]{nic}, null, null, null, "1")) {
            if (!cursor.moveToFirst()) return null;
            UserProfile profile = new UserProfile();
            profile.nic = cursor.getString(cursor.getColumnIndexOrThrow("nic"));
            profile.username = cursor.getString(cursor.getColumnIndexOrThrow("username"));
            profile.fullName = cursor.getString(cursor.getColumnIndexOrThrow("full_name"));
            profile.email = cursor.getString(cursor.getColumnIndexOrThrow("email"));
            profile.phone = cursor.getString(cursor.getColumnIndexOrThrow("phone"));
            profile.status = cursor.getString(cursor.getColumnIndexOrThrow("status"));
            profile.syncedAt = cursor.getLong(cursor.getColumnIndexOrThrow("synced_at"));
            return profile;
        }
    }

    // Replaces the cached booking list with the latest API page.
    public synchronized void replaceBookings(List<Booking> bookings, long syncedAt) {
        SQLiteDatabase db = getWritableDatabase();
        db.beginTransaction();
        try {
            db.delete("booking_history_cache", null, null);
            for (Booking booking : bookings) {
                ContentValues values = new ContentValues();
                values.put("id", booking.id);
                values.put("prosumer_nic", booking.prosumerNic);
                values.put("station_name", booking.stationName);
                values.put("scheduled_at", booking.scheduledAt);
                values.put("status", booking.status);
                values.put("synced_at", syncedAt);
                db.insertWithOnConflict("booking_history_cache", null, values, SQLiteDatabase.CONFLICT_REPLACE);
            }
            db.setTransactionSuccessful();
        } finally {
            db.endTransaction();
        }
    }

    // Returns the last booking list saved on this phone.
    public synchronized List<Booking> readBookings() {
        ArrayList<Booking> bookings = new ArrayList<>();
        try (Cursor cursor = getReadableDatabase().query("booking_history_cache", null, null, null, null, null, "scheduled_at ASC")) {
            while (cursor.moveToNext()) {
                Booking booking = new Booking();
                booking.id = cursor.getString(cursor.getColumnIndexOrThrow("id"));
                booking.prosumerNic = cursor.getString(cursor.getColumnIndexOrThrow("prosumer_nic"));
                booking.stationName = cursor.getString(cursor.getColumnIndexOrThrow("station_name"));
                booking.scheduledAt = cursor.getString(cursor.getColumnIndexOrThrow("scheduled_at"));
                booking.status = cursor.getString(cursor.getColumnIndexOrThrow("status"));
                booking.syncedAt = cursor.getLong(cursor.getColumnIndexOrThrow("synced_at"));
                bookings.add(booking);
            }
        }
        return bookings;
    }

    // Stores the latest prosumer dashboard so it can be shown offline.
    public synchronized void saveDashboard(DashboardSnapshot snapshot) {
        ContentValues values = new ContentValues();
        values.put("id", 1);
        values.put("pending_count", snapshot.pendingCount);
        values.put("approved_future_count", snapshot.approvedFutureCount);
        values.put("active_count", snapshot.activeCount);
        values.put("next_station", snapshot.nextStation);
        values.put("next_when", snapshot.nextWhen);
        values.put("synced_at", snapshot.syncedAt);
        getWritableDatabase().insertWithOnConflict("dashboard_snapshot", null, values, SQLiteDatabase.CONFLICT_REPLACE);
    }

    // Returns the last dashboard snapshot, or null when the phone has never synced.
    public synchronized DashboardSnapshot readDashboard() {
        try (Cursor cursor = getReadableDatabase().query("dashboard_snapshot", null, "id = 1", null, null, null, null, "1")) {
            if (!cursor.moveToFirst()) return null;
            DashboardSnapshot snapshot = new DashboardSnapshot();
            snapshot.pendingCount = cursor.getInt(cursor.getColumnIndexOrThrow("pending_count"));
            snapshot.approvedFutureCount = cursor.getInt(cursor.getColumnIndexOrThrow("approved_future_count"));
            snapshot.activeCount = cursor.getInt(cursor.getColumnIndexOrThrow("active_count"));
            snapshot.nextStation = cursor.getString(cursor.getColumnIndexOrThrow("next_station"));
            snapshot.nextWhen = cursor.getString(cursor.getColumnIndexOrThrow("next_when"));
            snapshot.syncedAt = cursor.getLong(cursor.getColumnIndexOrThrow("synced_at"));
            return snapshot;
        }
    }
}
