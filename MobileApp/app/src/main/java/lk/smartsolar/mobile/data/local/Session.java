/*
 * File: Session.java
 * Description: Immutable authenticated-session row used by SQLite and role routing.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.local;

public class Session {
    public final String token;
    public final long expiresAt;
    public final String role;
    public final String userId;
    public final String nic;

    public Session(String token, long expiresAt, String role, String userId, String nic) {
        this.token = token;
        this.expiresAt = expiresAt;
        this.role = role;
        this.userId = userId;
        this.nic = nic;
    }

    public boolean isUsable() {
        return token != null && !token.isEmpty() && expiresAt > System.currentTimeMillis();
    }
}
