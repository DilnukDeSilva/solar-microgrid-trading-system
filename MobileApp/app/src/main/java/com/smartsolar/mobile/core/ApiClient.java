/*
 * File: ApiClient.java
 * Description: Sends JSON requests to the Web API on a background thread and returns the answer on the UI thread.
 *              Stand-in for Member 1's app shell, written by Janukshan S (IT22635266) so the booking screens can run
 *              before the shell is merged.
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import com.smartsolar.mobile.R;

import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class ApiClient {

    public interface Callback {
        void onSuccess(String body);

        void onError(ApiException error);
    }

    private static final ExecutorService EXECUTOR = Executors.newFixedThreadPool(3);

    private final Handler mainThread = new Handler(Looper.getMainLooper());
    private final String baseUrl;
    private final Session session;

    // Reads the API address from config.xml and the token from the session.
    public ApiClient(Context context) {
        baseUrl = context.getString(R.string.api_base_url);
        session = new Session(context);
    }

    // Sends a request. The callback always runs on the UI thread.
    public void send(String method, String path, JSONObject body, Callback callback) {
        EXECUTOR.execute(() -> {
            try {
                String result = execute(method, path, body);
                mainThread.post(() -> callback.onSuccess(result));
            } catch (ApiException e) {
                mainThread.post(() -> callback.onError(e));
            }
        });
    }

    // Does the HTTP call and turns non-2xx answers into ApiException.
    private String execute(String method, String path, JSONObject body) throws ApiException {
        HttpURLConnection connection = null;
        try {
            connection = (HttpURLConnection) new URL(baseUrl + path).openConnection();
            connection.setRequestMethod(method);
            connection.setConnectTimeout(15000);
            connection.setReadTimeout(15000);
            connection.setRequestProperty("Accept", "application/json");

            String token = session.getToken();
            if (token != null) {
                connection.setRequestProperty("Authorization", "Bearer " + token);
            }

            if (body != null) {
                connection.setDoOutput(true);
                connection.setRequestProperty("Content-Type", "application/json; charset=utf-8");
                try (OutputStream out = connection.getOutputStream()) {
                    out.write(body.toString().getBytes(StandardCharsets.UTF_8));
                }
            }

            int status = connection.getResponseCode();
            InputStream stream = status >= 400 ? connection.getErrorStream() : connection.getInputStream();
            String text = readAll(stream);

            if (status >= 200 && status < 300) {
                return text;
            }
            throw ApiException.fromResponse(status, text);
        } catch (IOException e) {
            throw new ApiException(0, "NETWORK", "Cannot reach the server. Check your connection and try again.");
        } finally {
            if (connection != null) {
                connection.disconnect();
            }
        }
    }

    // Reads a response stream into a string.
    private static String readAll(InputStream stream) throws IOException {
        if (stream == null) {
            return "";
        }
        try (InputStream in = stream; ByteArrayOutputStream out = new ByteArrayOutputStream()) {
            byte[] buffer = new byte[4096];
            int read;
            while ((read = in.read(buffer)) != -1) {
                out.write(buffer, 0, read);
            }
            return out.toString("UTF-8");
        }
    }
}
