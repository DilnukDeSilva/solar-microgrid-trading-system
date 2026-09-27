/*
 * File: ApiClient.java
 * Description: Shared JSON API client with bearer auth, background I/O and contract error parsing.
 * Author: Dilnuk De Silva
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.remote;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import lk.smartsolar.mobile.BuildConfig;
import lk.smartsolar.mobile.data.local.Session;
import lk.smartsolar.mobile.data.local.SessionManager;
import lk.smartsolar.mobile.data.local.UserProfile;

public class ApiClient {
    private static volatile ApiClient instance;
    private final Context context;
    private final ExecutorService executor = Executors.newFixedThreadPool(3);
    private final Handler main = new Handler(Looper.getMainLooper());

    private ApiClient(Context context) {
        this.context = context.getApplicationContext();
    }

    public static ApiClient get(Context context) {
        if (instance == null) {
            synchronized (ApiClient.class) {
                if (instance == null) instance = new ApiClient(context);
            }
        }
        return instance;
    }

    public void login(String login, String password, ApiCallback<Session> callback) {
        JSONObject body = new JSONObject();
        put(body, "username", login);
        put(body, "password", password);
        request("POST", "auth/login", body, false, json -> {
            JSONObject user = json.optJSONObject("user");
            if (user == null) user = new JSONObject();
            long expiresAt;
            try {
                expiresAt = Instant.parse(json.getString("expiresAt")).toEpochMilli();
            } catch (Exception ignored) {
                expiresAt = System.currentTimeMillis() + 60L * 60L * 1000L;
            }
            Session session = new Session(
                    json.optString("token", ""), expiresAt,
                    user.optString("role", ""), user.optString("id", ""), user.optString("nic", ""));
            SessionManager.get(context).save(session);
            return session;
        }, callback);
    }

    public void register(String nic, String username, String password, String fullName, String email, String phone, ApiCallback<UserProfile> callback) {
        JSONObject body = profileBody(username, password, fullName, email, phone);
        put(body, "nic", nic);
        request("POST", "auth/register", body, false, UserProfile::fromJson, callback);
    }

    public void getMyProfile(ApiCallback<UserProfile> callback) {
        request("GET", "me", null, true, json -> {
            UserProfile profile = UserProfile.fromJson(json);
            SessionManager.get(context).database().saveProfile(profile);
            return profile;
        }, callback);
    }

    public void updateMyProfile(String username, String password, String fullName, String email, String phone, ApiCallback<UserProfile> callback) {
        JSONObject body = profileBody(username, password, fullName, email, phone);
        request("PUT", "me", body, true, json -> {
            UserProfile profile = UserProfile.fromJson(json);
            SessionManager.get(context).database().saveProfile(profile);
            return profile;
        }, callback);
    }

    public void requestDeactivation(ApiCallback<UserProfile> callback) {
        request("POST", "me/request-deactivation", null, true, UserProfile::fromJson, callback);
    }

    private JSONObject profileBody(String username, String password, String fullName, String email, String phone) {
        JSONObject body = new JSONObject();
        put(body, "username", username);
        if (password != null && !password.isEmpty()) put(body, "password", password);
        put(body, "fullName", fullName);
        put(body, "email", email);
        put(body, "phone", phone);
        return body;
    }

    private <T> void request(String method, String path, JSONObject body, boolean bearer, JsonMapper<T> mapper, ApiCallback<T> callback) {
        executor.execute(() -> {
            HttpURLConnection connection = null;
            try {
                String baseUrl = context.getSharedPreferences("settings", Context.MODE_PRIVATE)
                        .getString("api_base_url", BuildConfig.API_BASE_URL);
                if (!baseUrl.endsWith("/")) baseUrl += "/";
                connection = (HttpURLConnection) new URL(baseUrl + path).openConnection();
                connection.setRequestMethod(method);
                connection.setConnectTimeout(15000);
                connection.setReadTimeout(20000);
                connection.setRequestProperty("Accept", "application/json");

                if (bearer) {
                    Session session = SessionManager.get(context).current();
                    if (session == null) throw new ApiError(401, "UNAUTHORIZED", "Please sign in again.");
                    connection.setRequestProperty("Authorization", "Bearer " + session.token);
                }

                if (body != null) {
                    connection.setDoOutput(true);
                    connection.setRequestProperty("Content-Type", "application/json; charset=utf-8");
                    try (OutputStream output = connection.getOutputStream()) {
                        output.write(body.toString().getBytes(StandardCharsets.UTF_8));
                    }
                }

                int status = connection.getResponseCode();
                String text = read(status >= 200 && status < 300 ? connection.getInputStream() : connection.getErrorStream());
                JSONObject json = text.isEmpty() ? new JSONObject() : new JSONObject(text);
                if (status < 200 || status >= 300) {
                    if (status == 401) SessionManager.get(context).clear();
                    throw new ApiError(status, json.optString("code", "ERROR"), json.optString("message", "The request failed."));
                }

                T value = mapper.map(json);
                main.post(() -> callback.onSuccess(value));
            } catch (ApiError error) {
                main.post(() -> callback.onError(error));
            } catch (Exception error) {
                ApiError apiError = new ApiError(0, "NETWORK_ERROR", "Cannot reach the Smart Solar API. Check the API address and connection.");
                main.post(() -> callback.onError(apiError));
            } finally {
                if (connection != null) connection.disconnect();
            }
        });
    }

    private static String read(InputStream stream) throws Exception {
        if (stream == null) return "";
        StringBuilder result = new StringBuilder();
        try (BufferedReader reader = new BufferedReader(new InputStreamReader(stream, StandardCharsets.UTF_8))) {
            String line;
            while ((line = reader.readLine()) != null) result.append(line);
        }
        return result.toString();
    }

    private static void put(JSONObject object, String key, String value) {
        try { object.put(key, value == null ? "" : value.trim()); } catch (Exception ignored) { }
    }

    private interface JsonMapper<T> { T map(JSONObject json) throws Exception; }
}
