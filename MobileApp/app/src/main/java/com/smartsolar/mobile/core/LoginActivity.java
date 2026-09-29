/*
 * File: LoginActivity.java
 * Description: Logs in through POST /auth/login and opens the home screen. Stand-in for Member 1's login screen,
 *              written by Janukshan S (IT22635266) so the booking screens can run before the shell is merged.
 * Author: Member 1
 */

package com.smartsolar.mobile.core;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;

import com.smartsolar.mobile.R;

import org.json.JSONException;
import org.json.JSONObject;

public class LoginActivity extends AppCompatActivity {

    private EditText username;
    private EditText password;
    private Button loginButton;
    private ProgressBar progress;

    // Skips the form if a session already exists, otherwise shows it.
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (new Session(this).isLoggedIn()) {
            openHome();
            return;
        }

        setContentView(R.layout.activity_login);
        username = findViewById(R.id.username);
        password = findViewById(R.id.password);
        loginButton = findViewById(R.id.login_button);
        progress = findViewById(R.id.progress);
        loginButton.setOnClickListener(v -> login());
    }

    // Sends the credentials to the API and saves the session on success.
    private void login() {
        JSONObject body = new JSONObject();
        try {
            body.put("username", username.getText().toString().trim());
            body.put("password", password.getText().toString());
        } catch (JSONException e) {
            return;
        }

        setBusy(true);
        new ApiClient(this).send("POST", "auth/login", body, new ApiClient.Callback() {
            @Override
            public void onSuccess(String response) {
                setBusy(false);
                try {
                    new Session(LoginActivity.this).save(new JSONObject(response));
                    openHome();
                } catch (JSONException e) {
                    ErrorDialog.show(LoginActivity.this, new ApiException(0, "ERROR", "Unexpected answer from the server."));
                }
            }

            @Override
            public void onError(ApiException error) {
                setBusy(false);
                ErrorDialog.show(LoginActivity.this, error);
            }
        });
    }

    // Opens the home screen and closes the login screen.
    private void openHome() {
        startActivity(new Intent(this, HomeActivity.class));
        finish();
    }

    // Disables the button while the request is running.
    private void setBusy(boolean busy) {
        loginButton.setEnabled(!busy);
        progress.setVisibility(busy ? View.VISIBLE : View.GONE);
    }
}
