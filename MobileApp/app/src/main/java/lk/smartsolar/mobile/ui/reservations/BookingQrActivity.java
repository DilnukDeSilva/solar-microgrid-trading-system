/*
 * File: BookingQrActivity.java
 * Description: Shows the approved booking's qrToken as a QR code for the grid operator to scan.
 *              Reads the SQLite copy, so it also works without signal at the station.
 * Author: Janukshan S (IT22635266)
 */

package lk.smartsolar.mobile.ui.reservations;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.view.WindowManager;
import android.widget.ImageView;
import android.widget.TextView;

import com.google.zxing.BarcodeFormat;
import com.journeyapps.barcodescanner.BarcodeEncoder;

import lk.smartsolar.mobile.R;
import lk.smartsolar.mobile.data.reservations.MyReservationStore;
import lk.smartsolar.mobile.data.reservations.Reservation;
import lk.smartsolar.mobile.ui.BaseActivity;
import lk.smartsolar.mobile.util.TimeFormat;

public class BookingQrActivity extends BaseActivity {

    private static final String EXTRA_ID = "reservationId";

    // Opens the QR screen for one reservation.
    public static void open(Context context, String reservationId) {
        Intent intent = new Intent(context, BookingQrActivity.class);
        intent.putExtra(EXTRA_ID, reservationId);
        context.startActivity(intent);
    }

    // Draws the QR from the saved booking and keeps the screen bright so it scans easily.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_booking_qr);

        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        WindowManager.LayoutParams params = getWindow().getAttributes();
        params.screenBrightness = 1f;
        getWindow().setAttributes(params);

        Reservation r = new MyReservationStore(this).find(getIntent().getStringExtra(EXTRA_ID));
        TextView message = findViewById(R.id.qr_message);

        if (r == null || r.qrToken == null || r.qrToken.isEmpty() || !"Approved".equals(r.status)) {
            message.setText("The QR code appears once a grid operator approves the booking.");
            return;
        }

        ((TextView) findViewById(R.id.qr_station)).setText(r.stationName + "\n" + TimeFormat.full(r.scheduledAt));
        ((TextView) findViewById(R.id.qr_reference)).setText("Reference " + r.id);
        message.setText("Show this code to the grid operator at the station.");

        // Adapted from the zxing-android-embedded README ("Generate Barcode" example):
        // https://github.com/journeyapps/zxing-android-embedded#generate-barcode
        try {
            Bitmap bitmap = new BarcodeEncoder().encodeBitmap(r.qrToken, BarcodeFormat.QR_CODE, 800, 800);
            ((ImageView) findViewById(R.id.qr_image)).setImageBitmap(bitmap);
        } catch (Exception e) {
            message.setText("The QR code could not be drawn on this phone.");
        }
    }
}
