# Member 4: Booking Views, Dashboards & Operator Verification

Branch: `feature/m4-dashboards-operator` · Module owner for everything that **reads** bookings (lists, history, search, dashboards) and for the grid operator's QR verification and job completion.

## Marks you lead
- **Individual:** **Booking views & operational dashboards (10)**, Read QR code and mark the job done (2), **QR code scanning (2)**, SQLite (shared), Web↔API (2), Mobile↔API (2).
- **Group:** Home (index) page (1), shared UI style guide, README, compiling screenshots, video editing.

## Scope

### API (`ReservationQueryController`, `DashboardController`, `OperatorController` + services)
| Endpoint | Role | Rules |
|---|---|---|
| `GET /api/reservations?status&from&to&stationId&q&nic&page` | Prosumer (own only) / staff (all) | filters combine; sorted by `scheduledAt`; the server applies the prosumer's NIC from the token, never from the query |
| `GET /api/reservations/pending` | GridOperator, Backoffice | the operator approval queue (Approve calls M3's endpoint) |
| `GET /api/dashboard/me` | Prosumer | `pendingCount`, **`approvedFutureCount`**, `activeCount`, `nextReservation` |
| `GET /api/dashboard/operations` | Backoffice, GridOperator | today's bookings, pending approvals, active stations, completed today (for the web Home) |
| `POST /api/reservations/verify-qr` `{qrToken}` | GridOperator | token exists, status `Approved`, not used, for today or near its time → returns booking details; otherwise `QR_INVALID` / `QR_ALREADY_USED` |
| `POST /api/reservations/{id}/complete` | GridOperator | only after a successful verify; sets `Completed`, `completedAt`, `completedBy`; invalidates the token |

### Web (MVC + Bootstrap 5, in `WebApp/SmartSolar.Web`)
- **Home (index) dashboard:** the landing page after login shows live count cards per role, today's bookings and quick links. This is the shared style reference for the team (colours, cards, tables, alerts).
- **Booking monitor:** table with filters (status, date range, station, NIC/keyword), history tab, pagination.

### Android
- **Prosumer dashboard** (home tab): pending count, approved future count, next booking card, pull-to-refresh. All values come live from the API.
- **Bookings:** tabs for Current / Pending / History, plus a **search filter** (status, date range, station).
- **Operator mode** (GridOperator home): pending queue with Approve → **Scan QR** (camera permission, ZXing embedded scanner) → verify → details screen → **Finalise** → result screen.

### SQLite
`booking_history_cache` + `dashboard_snapshot`: show the last-known lists and counts offline, clearly labelled "last updated …", then refresh from the API.

## Roadmap
| When | Do |
|---|---|
| Sat 26 PM | List/filter + dashboard services. ZXing scan trial app. |
| Sun 27 | **List endpoint merged by 20:00** (M3's web page uses it). Verify + complete + operations dashboard. |
| Mon 28 | Web Home dashboard + booking monitor. Android dashboard on M1's shell. |
| Tue 29 | Bookings tabs + filter, operator scan flow, SQLite caches. Team LAN test at 18:00 (scan a QR from a second phone). |
| Wed 30 | README (Git link, contributions with commit links, video link), compile screenshots, edit the video. |

## Definition of done
Dashboard counts change when a booking is created, approved or cancelled (no hard-coded numbers). A reused or tampered QR is rejected. Filters combine correctly. A prosumer can never see another prosumer's bookings.

## Viva prep
Why verification happens on the server, what the QR contains (only the opaque token), why the token is single-use, how "approved future" is counted (status `Approved` and `scheduledAt` > now), and how prosumer scoping is enforced from the JWT.
