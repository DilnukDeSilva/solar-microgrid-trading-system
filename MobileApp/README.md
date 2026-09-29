# Smart Solar Android app

Native Java app, minimum SDK 24. Open this `MobileApp` directory in Android Studio and let Gradle sync.

- Emulator API URL: `http://10.0.2.2:5080/api/`
- Real phone: use **API settings** on the login page and enter the IIS/computer LAN URL ending in `/api/`.
- Cleartext HTTP is enabled because the assignment API is hosted on IIS over the lab LAN.

The shared SQLite database currently owns `session` and `user_profile`. Future modules must add tables through an additive `DbHelper.onUpgrade` migration and increment `DB_VERSION`.
