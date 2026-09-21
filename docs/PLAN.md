# Smart Solar Microgrid Trading System: Planning Document

## 1. Architecture (FAT service)

```
 Web App (ASP.NET Core MVC + Bootstrap 5)      Android App (Java/Kotlin, pure native)
   UI only, no DB, no business rules             UI + SQLite (login details, reference data)
              \                                   /
               \____ HTTP/JSON (REST, over LAN) _/
                              |
                    C# ASP.NET Core Web API  (hosted on Windows IIS)
                    ALL business rules live here
                              |
                          MongoDB (NoSQL)
```

- Clients never touch MongoDB. The web app and mobile app call the API only.
- LAN: IIS binds to `0.0.0.0:<port>`, with a static IP and a Windows Firewall inbound rule. The Android emulator reaches the host at `10.0.2.2`, and a real phone uses the PC's LAN IP.
- Auth: login endpoint returns a token (JWT), and the API checks the role (`Backoffice`, `GridOperator`, `Prosumer`) on every endpoint.

## 2. MongoDB collections (marked 1 mark each)

| Collection | Key fields |
|---|---|
| Users | `_id`/NIC (prosumers), username, passwordHash, role, fullName, email, phone, status (`Pending`/`Active`/`Deactivated`) |
| SolarStationInfo | stationId, name, latitude, longitude, capacityKwh, batterySlotsTotal, schedule, status |
| EnergyBookingSlots | slotId, stationId (ref), startTime, endTime, available flag |
| EnergyReservation | reservationId, prosumerNic (ref), stationId (ref), slotId (ref), scheduledAt, status (`Pending`/`Approved`/`Cancelled`/`Completed`), qrToken, createdAt |

Put sample data in a seed script (`mongosh`) so the marker sees populated collections.

## 3. Business rules (all in the API)

1. Only Backoffice can create/manage web users; Grid Operators get operational tools only.
2. NIC is the prosumer primary key. Deactivated accounts are reactivated only by Backoffice.
3. A station cannot be deactivated while active reservations exist.
4. A reservation must be scheduled within 7 days from now.
5. Update or cancel needs at least 12 hours' notice before the slot.
6. Approved reservation gets a secure QR token. The operator scan is verified server-side and the job is marked Completed.
7. Registration from mobile creates a Pending account, and Backoffice sees it in the "pending activation" view.

## 4. API endpoint list (agree the JSON contracts before coding)

- `POST /api/auth/login`, `POST /api/auth/register` (prosumer)
- `/api/users` CRUD, plus `/api/users/pending` and `/api/users/{nic}/activate|deactivate`
- `/api/stations` CRUD, `/api/stations/nearby?lat&lng&radius`, `/api/stations/{id}/slots`
- `/api/reservations` create/update/cancel/list, with filters (`status`, `from`, `to`, `q`)
- `/api/reservations/{id}/approve`, `/api/reservations/verify-qr`, `/api/reservations/{id}/complete`
- `/api/dashboard/prosumer/{nic}` (pending count, approved-future count)

## 5. Work split by vertical slice (each person owns API + client code for their features)

Each member writes the API endpoints for their own features, so everyone can defend end-to-end code at the viva.

| Member | Owns | Marks it earns |
|---|---|---|
| **1: Service & Users** | Solution skeleton, Mongo connection and DB design and seed, JWT auth, Users API, web login + role-based access + user management pages | Service Architecture (group), DB Design (group), Web login/roles/users |
| **2: Deployment & Web Operations** | **IIS deployment + LAN (needs Windows)**, web pages + API for Prosumers, Microgrid Nodes (GPS, capacity, slots, deactivation rule), Reservations (7-day/12-hour rules), web UI polish and Home page | Node Mgmt, Slot Booking |
| **3: Mobile Account & Booking** | Android project, **SQLite** layer, register/login/role home, edit/deactivate profile, create/update/cancel booking + summary screen, web "pending activation" view | Mobile Auth, Reservation workflow |
| **4: Mobile Views, Map & Operator** | Dashboard (pending count, approved-future count), current/pending/history, search filters, **Google Maps** nearby nodes, **QR generation + scanning**, operator finalise flow | Booking views, Operator & Maps |

Shared: report, diagrams, README, and the video (split by member).

**Dependency order:** M1 ships skeleton + auth + DB in about 2 days. Then M2, M3 and M4 work in parallel against Swagger, using mock JSON until endpoints land.

## 6. Suggested schedule (today 20 Sep, deadline 30 Sep 11:59 PM)

- 20–21 Sep: repo, JSON contracts, DB + skeleton + auth (M1); UI mockups (all)
- 22–26 Sep: feature slices in parallel, with small descriptive commits
- 27–28 Sep: integration on LAN/IIS, bug fixing
- 29 Sep: report, diagrams (high-level, use case, DFD), screenshots, README, video (≤5 min)
- 30 Sep: zip as `ITxxxxxxxx.zip` and submit early

## 7. Marking traps to avoid

- **Comment header block on every `.cs` file**, and an inline comment at the start of every method. Uncommented code isn't marked.
- Reference any copied snippet in a comment (tutorial, Stack Overflow), or it counts as plagiarism.
- Nothing may talk to MongoDB directly from the web app or mobile app.
- The Android app must be pure native, with no Flutter, React Native or Xamarin. Use `SQLiteOpenHelper` (Room is arguably a framework, so check with your lecturer).
- Git: meaningful commits from all four members, since the README must show individual contributions.
- README needs the Git link, the ≤5 min video link, and per-member contributions. The report needs all the items on page 3 of the brief.
- Include unique screenshots of the app's main screen.

## 8. Tools to learn first (from your lecture notes)

ASP.NET Core Web API, MongoDB.Driver, IIS + hosting bundle, MVC + Bootstrap 5, Android SQLite/`SQLiteOpenHelper`, Google Maps SDK, a QR library (ZXing).
