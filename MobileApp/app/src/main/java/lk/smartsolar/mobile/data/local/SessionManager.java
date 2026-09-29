/*
 * File: SessionManager.java
 * Description: Persistent login state backed by the shared SQLite database.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.local;

import android.content.Context;

public class SessionManager {
    private static volatile SessionManager instance;
    private final DbHelper db;

    private SessionManager(Context context) {
        db = new DbHelper(context);
    }

    public static SessionManager get(Context context) {
        if (instance == null) {
            synchronized (SessionManager.class) {
                if (instance == null) instance = new SessionManager(context.getApplicationContext());
            }
        }
        return instance;
    }

    public Session current() {
        Session session = db.readSession();
        if (session != null && !session.isUsable()) {
            db.clearSession();
            return null;
        }
        return session;
    }

    public void save(Session session) { db.saveSession(session); }
    public void clear() { db.clearSession(); }
    public DbHelper database() { return db; }
}
