/*
 * File: ReservationApi.java
 * Description: Reservation calls to the Web API. Turns the JSON answers into objects for the screens.
 * Author: Janukshan S (IT22635266)
 */

package com.smartsolar.mobile.reservations;

import android.content.Context;
import android.net.Uri;

import com.smartsolar.mobile.core.ApiClient;
import com.smartsolar.mobile.core.ApiException;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public class ReservationApi {

    public interface Result<T> {
        void onSuccess(T value);

        void onError(ApiException error);
    }

    private interface Parser<T> {
        T parse(String body) throws JSONException;
    }

    private final ApiClient client;

    // Uses the shared API client, which adds the login token.
    public ReservationApi(Context context) {
        client = new ApiClient(context);
    }

    // GET /reservations/bookable-stations
    public void getBookableStations(Result<List<BookingStation>> result) {
        client.send("GET", "reservations/bookable-stations", null, handle(result, body -> {
            JSONArray array = new JSONArray(body);
            List<BookingStation> stations = new ArrayList<>();
            for (int i = 0; i < array.length(); i++) {
                JSONObject item = array.getJSONObject(i);
                stations.add(new BookingStation(item.getString("id"), item.getString("name")));
            }
            return stations;
        }));
    }

    // GET /reservations/available-slots. date is "yyyy-MM-dd" or null for every day.
    public void getAvailableSlots(String stationId, String date, Result<List<AvailableSlot>> result) {
        String path = "reservations/available-slots?stationId=" + Uri.encode(stationId);
        if (date != null) {
            path += "&date=" + date;
        }

        client.send("GET", path, null, handle(result, body -> {
            JSONArray array = new JSONArray(body);
            List<AvailableSlot> slots = new ArrayList<>();
            for (int i = 0; i < array.length(); i++) {
                JSONObject item = array.getJSONObject(i);
                slots.add(new AvailableSlot(item.getString("id"), item.getString("startTime"), item.getString("endTime")));
            }
            return slots;
        }));
    }

    // POST /reservations. The API takes the prosumer's NIC from the login token.
    public void create(String stationId, String slotId, Result<String> result) {
        JSONObject body = new JSONObject();
        try {
            body.put("stationId", stationId);
            body.put("slotId", slotId);
        } catch (JSONException e) {
            return;
        }
        client.send("POST", "reservations", body, handle(result, response -> response));
    }

    // Parses a successful answer, or passes the API error on.
    private <T> ApiClient.Callback handle(Result<T> result, Parser<T> parser) {
        return new ApiClient.Callback() {
            @Override
            public void onSuccess(String body) {
                T value;
                try {
                    value = parser.parse(body);
                } catch (JSONException e) {
                    result.onError(new ApiException(0, "ERROR", "Unexpected answer from the server."));
                    return;
                }
                result.onSuccess(value);
            }

            @Override
            public void onError(ApiException error) {
                result.onError(error);
            }
        };
    }
}
