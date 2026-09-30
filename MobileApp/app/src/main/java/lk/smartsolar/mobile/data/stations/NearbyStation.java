/*
 * File: NearbyStation.java
 * Description: One station returned by GET /stations/nearby, including distance and free slots.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */
package lk.smartsolar.mobile.data.stations;

import org.json.JSONObject;

import java.util.Locale;

public class NearbyStation {

    public String id;
    public String name;
    public double latitude;
    public double longitude;
    public double capacityKwh;
    public int batterySlotsTotal;
    public double distanceKm;
    public int freeSlots;

    // Reads one station object from the nearby-stations JSON.
    public static NearbyStation fromJson(JSONObject json) {
        NearbyStation station = new NearbyStation();
        station.id = json.optString("id");
        station.name = json.optString("name");
        station.latitude = json.optDouble("latitude");
        station.longitude = json.optDouble("longitude");
        station.capacityKwh = json.optDouble("capacityKwh");
        station.batterySlotsTotal = json.optInt("batterySlotsTotal");
        station.distanceKm = json.optDouble("distanceKm");
        station.freeSlots = json.optInt("freeSlots");
        return station;
    }

    // List row: "Malabe Solar Hub · 0.01 km · 18 free".
    @Override
    public String toString() {
        return name + " · " + String.format(Locale.US, "%.2f", distanceKm) + " km · " + freeSlots + " free";
    }
}
