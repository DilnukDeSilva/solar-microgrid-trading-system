/*
 * File: ReservationApi.java
 * Description: Reservation calls to the Web API. Turns the JSON answers into objects for the screens
 *              and keeps a copy of each returned booking in SQLite.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.data.reservations;

import android.content.Context;
import android.net.Uri;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;

public class ReservationApi {

    private final ApiClient client;
    private final MyReservationStore store;

    // Uses the shared API client, which adds the login token, and the local copy in SQLite.
    public ReservationApi(Context context) {
        client = ApiClient.get(context);
        store = new MyReservationStore(context);
    }

    // GET /reservations/bookable-stations
    public void getBookableStations(ApiCallback<List<BookingStation>> callback) {
        client.send("GET", "reservations/bookable-stations", null, body -> {
            JSONArray array = new JSONArray(body);
            List<BookingStation> stations = new ArrayList<>();
            for (int i = 0; i < array.length(); i++) {
                JSONObject item = array.getJSONObject(i);
                stations.add(new BookingStation(item.getString("id"), item.getString("name")));
            }
            return stations;
        }, callback);
    }

    // GET /reservations/available-slots. date is "yyyy-MM-dd" or null for every day.
    public void getAvailableSlots(String stationId, String date, ApiCallback<List<AvailableSlot>> callback) {
        String path = "reservations/available-slots?stationId=" + Uri.encode(stationId);
        if (date != null) {
            path += "&date=" + date;
        }

        client.send("GET", path, null, body -> {
            JSONArray array = new JSONArray(body);
            List<AvailableSlot> slots = new ArrayList<>();
            for (int i = 0; i < array.length(); i++) {
                JSONObject item = array.getJSONObject(i);
                slots.add(new AvailableSlot(item.getString("id"), item.getString("startTime"), item.getString("endTime")));
            }
            return slots;
        }, callback);
    }

    // POST /reservations. The API takes the prosumer's NIC from the login token. Returns the raw reservation JSON.
    public void create(String stationId, String slotId, ApiCallback<String> callback) {
        JSONObject body = new JSONObject();
        try {
            body.put("stationId", stationId);
            body.put("slotId", slotId);
        } catch (Exception ignored) {
        }
        client.send("POST", "reservations", body, this::keepCopy, callback);
    }

    // GET /reservations/{id}
    public void get(String id, ApiCallback<String> callback) {
        client.send("GET", "reservations/" + Uri.encode(id), null, this::keepCopy, callback);
    }

    // PUT /reservations/{id}. Moves the booking to another slot; the API checks the 12-hour and 7-day rules.
    public void update(String id, String stationId, String slotId, ApiCallback<String> callback) {
        JSONObject body = new JSONObject();
        try {
            body.put("stationId", stationId);
            body.put("slotId", slotId);
        } catch (Exception ignored) {
        }
        client.send("PUT", "reservations/" + Uri.encode(id), body, this::keepCopy, callback);
    }

    // POST /reservations/{id}/cancel. The API checks the 12-hour rule.
    public void cancel(String id, ApiCallback<String> callback) {
        client.send("POST", "reservations/" + Uri.encode(id) + "/cancel", new JSONObject(), this::keepCopy, callback);
    }

    // Saves every reservation the API returns into SQLite, then passes the JSON on to the screen.
    private String keepCopy(String body) throws Exception {
        store.save(Reservation.fromJson(new JSONObject(body)));
        return body;
    }
}
