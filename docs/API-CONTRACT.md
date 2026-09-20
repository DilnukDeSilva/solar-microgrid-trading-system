# Shared API Contract (v1): all 4 members must agree on this on Day 1

Change this file only through a Git pull request that all four members approve. It is the single source of truth for JSON shapes, so clients can be built against mock data before the API exists.

## Conventions
- Base URL: `http://<host-ip>:<port>/api` (emulator: `10.0.2.2`).
- JSON `camelCase`. Dates are ISO-8601 UTC strings (`2026-09-25T10:00:00Z`). The clients convert to local time for display.
- Auth: `Authorization: Bearer <jwt>`. The JWT claims are `sub` (user id), `role`, `nic` (prosumers only).
- Roles: `Backoffice`, `GridOperator`, `Prosumer`.
- Enums (strings):
  - user status: `Pending` | `Active` | `Deactivated`
  - station status: `Active` | `Inactive`
  - reservation status: `Pending` | `Approved` | `Cancelled` | `Completed`
- Error body for every non-2xx: `{ "code": "RULE_12H", "message": "Updates need 12 hours notice" }`
  - Status codes: 400 validation, 401 no/invalid token, 403 wrong role, 404 missing, 409 business-rule conflict.
  - Rule codes: `RULE_7DAYS`, `RULE_12H`, `STATION_HAS_RESERVATIONS`, `SLOT_TAKEN`, `NIC_EXISTS`, `ACCOUNT_NOT_ACTIVE`, `QR_INVALID`, `QR_ALREADY_USED`.
- Every response is data only, with no HTML. All rules are enforced server-side; clients only display the error `message`.

## Data shapes

**User**
`{ id, nic|null, username, fullName, email, phone, role, status, createdAt }`. The password hash is never returned. `nic` is required for `Prosumer` and is the primary key for prosumers.

**Station** (SolarStationInfo)
`{ id, name, latitude, longitude, capacityKwh, batterySlotsTotal, schedule: [{ dayOfWeek, openTime, closeTime }], status }`

**Slot** (EnergyBookingSlots)
`{ id, stationId, startTime, endTime, isAvailable }`

**Reservation** (EnergyReservation)
`{ id, prosumerNic, stationId, stationName, slotId, scheduledAt, status, qrToken|null, createdAt, updatedAt, completedAt|null, completedBy|null }`

## Endpoints (owner in brackets)

| Method + path | Role | Owner |
|---|---|---|
| `GET /health` | anon | M1 |
| `POST /auth/login` `{username|nic, password}` → `{token, expiresAt, user}` | anon | M1 |
| `GET /users`, `GET /users/{id}`, `POST /users`, `PUT /users/{id}`, `POST /users/{id}/deactivate` (staff accounts) | Backoffice | M1 |
| `GET /prosumers?status&q`, `GET /prosumers/{nic}`, `POST /prosumers`, `PUT /prosumers/{nic}`, `POST /prosumers/{nic}/deactivate`, `POST /prosumers/{nic}/reactivate` (Backoffice only) | Backoffice/GridOperator | M2 |
| `GET /stations`, `GET /stations/{id}`, `POST /stations`, `PUT /stations/{id}`, `POST /stations/{id}/deactivate` | Backoffice (read: all) | M2 |
| `GET /stations/{id}/slots`, `POST /stations/{id}/slots`, `PUT /slots/{id}`, `DELETE /slots/{id}` | Backoffice/GridOperator | M2 |
| `POST /reservations`, `PUT /reservations/{id}`, `POST /reservations/{id}/cancel`, `GET /reservations/{id}` | Prosumer (own) / Operator | M2 |
| `POST /auth/register` (prosumer, creates `Pending`) | anon | M3 |
| `GET /me`, `PUT /me`, `POST /me/request-deactivation` | Prosumer | M3 |
| `GET /prosumers/pending`, `POST /prosumers/{nic}/activate` | Backoffice | M3 |
| `GET /reservations?status&from&to&q&nic` (list, history, filters) | Prosumer (own) / staff (all) | M4 |
| `GET /reservations/pending` | Operator | M4 |
| `POST /reservations/{id}/approve` → sets `qrToken` | Operator/Backoffice | M4 |
| `POST /reservations/verify-qr` `{qrToken}` → reservation details | Operator | M4 |
| `POST /reservations/{id}/complete` | Operator | M4 |
| `GET /dashboard/me` → `{pendingCount, approvedFutureCount, nextReservation}` | Prosumer | M4 |
| `GET /stations/nearby?lat&lng&radiusKm` | any auth | M4 |

Notes:
- To avoid merge conflicts, each member puts their endpoints in **their own controller files** (for example `NearbyStationsController`, `ReservationQueryController`).
- Anything that changes the Users collection uses the shared `UserRepository` written by M1.
- "Owner" means writes the API code, the service logic and the tests for that endpoint.

## Business rule table (implemented once, in the service layer)

| Rule | Where | Owner |
|---|---|---|
| Reservation `scheduledAt` ≤ now + 7 days and in the future | create | M2 |
| Update/cancel only if `scheduledAt` − now ≥ 12 h | update, cancel | M2 |
| Slot must be available (no double booking) | create, update | M2 |
| Station deactivation blocked with Pending/Approved future reservations | station deactivate | M2 |
| Deactivated prosumer reactivated by Backoffice only | reactivate | M2 |
| Only `Active` prosumers may reserve | create | M2 |
| New mobile accounts start `Pending` | register | M3 |
| Only `Approved` reservations produce a QR; token is single-use | approve, verify, complete | M4 |

## Seed data everyone can rely on (M1 provides on Day 2)
- Users: `admin / Admin@123` (Backoffice), `operator1 / Oper@123` (GridOperator), prosumers with NICs `200012345678` (Active) and `199912345678` (Pending).
- 3 stations around Colombo/Malabe with 4 slots each on the next 5 days.
- A handful of reservations in each status, including one in the past.
