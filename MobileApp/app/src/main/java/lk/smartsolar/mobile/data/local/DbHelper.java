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

public class DbHelper extends SQLiteOpenHelper {
    private static final String DB_NAME = "smart_solar.db";
    // Every module adds its CREATE statement and increments this version.
    private static final int DB_VERSION = 1;

    public DbHelper(Context context) {
        super(context.getApplicationContext(), DB_NAME, null, DB_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL("CREATE TABLE session (token TEXT NOT NULL, expires_at INTEGER NOT NULL, role TEXT NOT NULL, user_id TEXT NOT NULL, nic TEXT)");
        db.execSQL("CREATE TABLE user_profile (nic TEXT PRIMARY KEY, username TEXT NOT NULL, full_name TEXT NOT NULL, email TEXT NOT NULL, phone TEXT NOT NULL, status TEXT NOT NULL, synced_at INTEGER NOT NULL)");
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        // Additive migrations belong here. Never drop other modules' offline data.
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
}
