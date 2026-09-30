/*
 * File: StationApi.java
 * Description: Nearby-station calls. The map and the list both use this.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */
package lk.smartsolar.mobile.data.stations;

import android.content.Context;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

import lk.smartsolar.mobile.data.remote.ApiCallback;
import lk.smartsolar.mobile.data.remote.ApiClient;

public class StationApi {

    private final ApiClient client;

    // Uses the shared API client, which adds the login token.
    public StationApi(Context context) {
        client = ApiClient.get(context);
    }

    // GET /stations/nearby. radiusKm is 25 so the map covers Colombo from a phone in Malabe.
    public void getNearby(double lat, double lng, ApiCallback<List<NearbyStation>> callback) {
        String path = "stations/nearby?lat=" + lat + "&lng=" + lng + "&radiusKm=25";
        client.send("GET", path, null, body -> {
            JSONArray array = new JSONArray(body);
            List<NearbyStation> stations = new ArrayList<>();
            for (int i = 0; i < array.length(); i++) {
                JSONObject item = array.getJSONObject(i);
                stations.add(NearbyStation.fromJson(item));
            }
            return stations;
        }, callback);
    }
}
