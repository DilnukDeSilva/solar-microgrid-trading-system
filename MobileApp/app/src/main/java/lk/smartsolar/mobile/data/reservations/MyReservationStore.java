/*
 * File: MyReservationStore.java
 * Description: Reads and writes the my_reservations SQLite table. The API is the source of truth;
 *              this is a read-only copy so the booking and its QR can be shown without signal.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.data.reservations;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import lk.smartsolar.mobile.data.local.DbHelper;
import lk.smartsolar.mobile.data.local.SessionManager;

public class MyReservationStore {

    private final DbHelper db;

    // Uses the app's shared SQLite database.
    public MyReservationStore(Context context) {
        db = SessionManager.get(context).database();
    }

    // Inserts or replaces the local copy of a booking with what the API just returned.
    public void save(Reservation reservation) {
        ContentValues values = new ContentValues();
        values.put("id", reservation.id);
        values.put("station_name", reservation.stationName);
        values.put("slot_start", reservation.scheduledAt);
        values.put("status", reservation.status);
        values.put("qr_token", reservation.qrToken);
        values.put("synced_at", System.currentTimeMillis());
        db.getWritableDatabase().insertWithOnConflict("my_reservations", null, values, SQLiteDatabase.CONFLICT_REPLACE);
    }

    // Returns the saved copy of a booking, or null if this phone has never loaded it.
    public Reservation find(String id) {
        try (Cursor cursor = db.getReadableDatabase().query("my_reservations", null, "id = ?", new String[]{id}, null, null, null)) {
            if (!cursor.moveToFirst()) {
                return null;
            }
            Reservation r = new Reservation();
            r.id = cursor.getString(cursor.getColumnIndexOrThrow("id"));
            r.stationName = cursor.getString(cursor.getColumnIndexOrThrow("station_name"));
            r.scheduledAt = cursor.getString(cursor.getColumnIndexOrThrow("slot_start"));
            r.status = cursor.getString(cursor.getColumnIndexOrThrow("status"));
            r.qrToken = cursor.getString(cursor.getColumnIndexOrThrow("qr_token"));
            r.syncedAt = cursor.getLong(cursor.getColumnIndexOrThrow("synced_at"));
            return r;
        }
    }
}
