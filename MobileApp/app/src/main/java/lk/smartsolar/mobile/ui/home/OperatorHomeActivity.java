/*
 * File: OperatorHomeActivity.java
 * Description: Grid Operator home with the approval queue, QR scanning and server-side verification.
 * Author: Herath D M S T (IT22639776)
 */

package lk.smartsolar.mobile.ui.home;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.journeyapps.barcodescanner.ScanContract;
import com.journeyapps.barcodescanner.ScanOptions;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.local.Booking;
import lk.smartsolar.mobile.data.remote.ApiClient;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.ui.auth.LogoutActivity;
import lk.smartsolar.mobile.ui.operator.PendingQueueActivity;
import lk.smartsolar.mobile.ui.operator.QrDetailsActivity;

public class OperatorHomeActivity extends BaseActivity {
    private static final int CAMERA_REQUEST = 41;

    private final androidx.activity.result.ActivityResultLauncher<ScanOptions> scanner =
            registerForActivityResult(new ScanContract(), result -> {
                if (result.getContents() != null) verify(result.getContents());
            });

    // Opens the approval queue, the camera scanner and logout.
    @Override
    protected void onCreate(Bundle state) {
        super.onCreate(state);
        setContentView(R.layout.activity_operator_home);
        findViewById(R.id.pendingButton).setOnClickListener(v -> startActivity(new Intent(this, PendingQueueActivity.class)));
        findViewById(R.id.scanButton).setOnClickListener(v -> ensureCamera());
        findViewById(R.id.logoutButton).setOnClickListener(v -> startActivity(new Intent(this, LogoutActivity.class)));
    }

    // Asks for the camera permission before opening the scanner.
    private void ensureCamera() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED) {
            startScan();
        } else {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.CAMERA}, CAMERA_REQUEST);
        }
    }

    // Starts the embedded ZXing QR scanner. The code it reads is only the booking token.
    private void startScan() {
        ScanOptions options = new ScanOptions();
        options.setDesiredBarcodeFormats(ScanOptions.QR_CODE);
        options.setPrompt("Scan the booking QR");
        options.setBeepEnabled(true);
        options.setOrientationLocked(false);
        scanner.launch(options);
    }

    // Sends the scanned token to the API and opens the details screen on success.
    private void verify(String token) {
        showLoading(true);
        ApiClient.get(this).verifyQr(token, new lk.smartsolar.mobile.data.remote.ApiCallback<Booking>() {
            @Override
            public void onSuccess(Booking booking) {
                showLoading(false);
                Intent intent = new Intent(OperatorHomeActivity.this, QrDetailsActivity.class);
                intent.putExtra("id", booking.id);
                intent.putExtra("station", booking.stationName);
                intent.putExtra("nic", booking.prosumerNic);
                intent.putExtra("when", booking.scheduledAt);
                intent.putExtra("status", booking.status);
                startActivity(intent);
            }

            @Override
            public void onError(lk.smartsolar.mobile.data.remote.ApiError error) {
                showLoading(false);
                showError(error);
            }
        });
    }

    // Continues the scan after the operator allows the camera.
    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == CAMERA_REQUEST && grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            startScan();
        }
    }
}
