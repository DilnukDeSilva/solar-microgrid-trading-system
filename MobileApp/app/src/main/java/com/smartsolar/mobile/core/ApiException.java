/*
 * File: ApiException.java
 * Description: An error from the API ({code, message}) or a network failure. Stand-in for Member 1's app shell,
 *              written by Janukshan S (IT22635266) so the booking screens can run before the shell is merged.
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import org.json.JSONObject;

public class ApiException extends Exception {

    private final int status;
    private final String code;

    // Keeps the HTTP status, the API error code and the message to show the user.
    public ApiException(int status, String code, String message) {
        super(message);
        this.status = status;
        this.code = code;
    }

    // Builds the error from the API's {code, message} body, with a fallback if the body is not JSON.
    public static ApiException fromResponse(int status, String body) {
        try {
            JSONObject json = new JSONObject(body);
            return new ApiException(status, json.optString("code", "ERROR"), json.optString("message", "The request failed."));
        } catch (Exception e) {
            return new ApiException(status, "ERROR", "The request failed (HTTP " + status + ").");
        }
    }

    public int getStatus() {
        return status;
    }

    public String getCode() {
        return code;
    }
}
