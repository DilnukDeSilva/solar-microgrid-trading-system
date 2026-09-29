/*
 * File: HomeActivity.java
 * Description: Simple prosumer home with links to the booking screens. Stand-in for Member 1's role home,
 *              written by Janukshan S (IT22635266) so the booking screens can run before the shell is merged.
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.smartsolar.mobile.R;
import com.smartsolar.mobile.reservations.BookSlotActivity;

public class HomeActivity extends AppCompatActivity {

    // Shows the user's name and the menu buttons.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        Session session = new Session(this);
        TextView welcome = findViewById(R.id.welcome);
        welcome.setText("Hello, " + session.getFullName());

        findViewById(R.id.book_button).setOnClickListener(v -> startActivity(new Intent(this, BookSlotActivity.class)));
        findViewById(R.id.logout_button).setOnClickListener(v -> {
            session.clear();
            startActivity(new Intent(this, LoginActivity.class));
            finish();
        });
    }
}
