/*
 * File: BookingStation.java
 * Description: A station that is taking bookings, shown in the station picker.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.data.reservations;

public class BookingStation {

    public final String id;
    public final String name;

    // Keeps the station id and display name.
    public BookingStation(String id, String name) {
        this.id = id;
        this.name = name;
    }

    // The spinner shows this text.
    @Override
    public String toString() {
        return name;
    }
}
