/*
 * File: ApiError.java
 * Description: API contract error represented as an exception for UI callbacks.
 * Author: Dilnuk De Silva
 * Created: 27/09/2026
 */
package lk.smartsolar.mobile.data.remote;

public class ApiError extends Exception {
    public final int status;
    public final String code;

    public ApiError(int status, String code, String message) {
        super(message);
        this.status = status;
        this.code = code;
    }
}
