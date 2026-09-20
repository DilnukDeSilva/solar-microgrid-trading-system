# Member 4: Android Dashboard, Views, Maps, QR & Operator Mode

## Marks you own
- Individual: Booking Views & Operational Dashboards (10), Grid Operator Verification & Map Features (7), Google Maps (3), QR scanning (2), share of Mobile ↔ API
- Group: Mobile interfaces (share with M3)

## Blocked until
- M3's app shell (target Day 3): navigation, API client, session, SQLite helper.
- M1's login and seed data (Day 2). M2's reservations exist by about Day 5.

## Milestone 0: spikes you can do immediately (Day 1–3)
Nothing needs to be finished by anyone else for these:
- Get a **Google Maps API key**, enable Maps SDK for Android, and render a blank map in a scratch project.
- Prove QR **generation** and **scanning** with ZXing (or the ML Kit/CameraX barcode scanner) in a scratch app, including the camera permission flow.
- Decide the QR payload with M1/M2: use the opaque `qrToken` plus reservation id, **no personal data in the QR**.
- Read the dashboard/filter contract and prototype the screens with mock JSON.

## Milestone 1: API side (your own controllers, Day 3–6)
- `GET /reservations` with filters (`status`, `from`, `to`, `q`, `nic`); prosumers see only their own.
- `GET /reservations/pending` (operator queue).
- `POST /reservations/{id}/approve` → generates the secure random `qrToken` (Pending → Approved).
- `POST /reservations/verify-qr` → checks token, approved state, not used (`QR_INVALID`, `QR_ALREADY_USED`).
- `POST /reservations/{id}/complete` → marks Completed with operator id and time.
- `GET /dashboard/me` → `pendingCount`, `approvedFutureCount`, `nextReservation`.
- `GET /stations/nearby?lat&lng&radiusKm` (distance calculation on the server).

## Milestone 2: mobile screens (Day 4–8)
- **Prosumer dashboard**: pending count, approved-future count, next booking, all read live from the API.
- Current/pending bookings list, booking history, and **search/filter** by status, date range, station/keyword.
- **QR screen**: once a booking is Approved, show the QR for the prosumer.
- **Map**: nearby stations plotted from stored lat/lng, tap a marker to see details (name, capacity, free slots). Use device location with permission handling.
- **Operator mode** (for GridOperator login): scan QR → call verify → show reservation details → confirm to finalise → call complete → result screen. Also show the pending queue and approve action.

## Depends on / blocks
- Depends on: M3 (shell), M1 (auth/seed), M2 (reservations to display).
- Blocks: nobody, so you are last in the chain. Because of this, do your spikes and API endpoints early so nothing lands in the final days.

## Definition of done
- Dashboard counts change when reservations change, with no hard-coded numbers.
- A QR scanned by a second device verifies against the server, and a reused or tampered QR is rejected with the contract error.
- The map shows markers from the API, not a fixed list.
- Comment header and inline comments on all files. Cite any snippet you didn't write, especially map/scanner examples.

## Viva prep: be able to explain
How the QR token is generated and why it's single-use, what's inside the QR and what's deliberately not, how nearby distance is computed, how permissions are requested, why verification happens on the server and not in the app.
