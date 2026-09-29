/*
 * File: AvailableSlot.java
 * Description: A free slot returned by the API for the slot list.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.data.reservations;

import lk.smartsolar.mobile.util.TimeFormat;

public class AvailableSlot {

    public final String id;
    public final String startTime;
    public final String endTime;

    // Keeps the slot id and its UTC start and end times.
    public AvailableSlot(String id, String startTime, String endTime) {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
    }

    // Text shown in the slot list, e.g. "Mon 28 Sep  08:00 - 10:00".
    @Override
    public String toString() {
        return TimeFormat.day(startTime) + "   " + TimeFormat.time(startTime) + " - " + TimeFormat.time(endTime);
    }
}
