package lk.smartsolar.mobile.data.remote;

public interface ApiCallback<T> {
    void onSuccess(T value);
    void onError(ApiError error);
}
