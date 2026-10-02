# M3 Implementation Guide: Energy Reservations & QR Dispatch

---

## 1. Why this module matters

Reservations are the core of the system. Most of the spec's business rules (7-day window, 12-hour notice, approval → QR, slot availability) run through your service. It feeds M4's dashboards and operator flow and uses M2's stations and slots. Built well, it is also the clearest example of the **FAT service** idea: the same `ReservationService` enforces the same rules whether the call comes from the web or from Android.

## 2. Requirements

### Functional

| ID   | Requirement                                                                                           | Client(s)                                         |
| ---- | ----------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| FR-1 | A prosumer books a free slot at an active station.                                                    | Android                                           |
| FR-2 | Staff (Backoffice / GridOperator) book on behalf of a prosumer by NIC.                                | Web                                               |
| FR-3 | The owner or staff move a booking to another free slot.                                               | Android, Web                                      |
| FR-4 | The owner or staff cancel a booking.                                                                  | Android, Web                                      |
| FR-5 | A GridOperator or Backoffice user approves a pending booking, which generates a transaction QR token. | Web (+ M4's mobile queue calls the same endpoint) |
| FR-6 | After create, update and cancel, a **summary page** shows station, slot time, status and reference.   | Android, Web                                      |
| FR-7 | The prosumer views the QR for an approved booking, including offline.                                 | Android                                           |
| FR-8 | Get one booking by id; a prosumer only sees their own.                                                | both                                              |

### Business rules (all enforced in `ReservationService`, never in a client)

| ID    | Rule                                                                                                                | Error (HTTP)                                                    |
| ----- | ------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| BR-1  | The slot's start time is **in the future and ≤ now + 7 days**. Applies to create **and** to the new slot on update. | `RULE_7DAYS` (409)                                              |
| BR-2  | Update or cancel only if the **current** booking starts **≥ 12 hours** from now.                                    | `RULE_12H` (409)                                                |
| BR-3  | The slot must be available; claiming it must be atomic, so two requests cannot book the same slot.                  | `SLOT_TAKEN` (409)                                              |
| BR-4  | The prosumer must be `Active`.                                                                                      | `ACCOUNT_NOT_ACTIVE` (409)                                      |
| BR-5  | The station must be `Active` and the slot must belong to it.                                                        | `VALIDATION_ERROR` (400)                                        |
| BR-6  | Only `Pending` or `Approved` bookings can be updated or cancelled; `Completed`/`Cancelled` are final.               | `INVALID_STATE` (409), a new code: add it to the contract by PR |
| BR-7  | Only `Pending` future bookings can be approved. Approval creates a cryptographically random, unguessable `qrToken`. | `INVALID_STATE` (409)                                           |
| BR-8  | Updating an `Approved` booking sends it back to `Pending` and clears its `qrToken` (the old QR must stop working).  | —                                                               |
| BR-9  | A prosumer can act only on their own bookings (NIC taken from the **JWT**, never from the request body).            | 403                                                             |
| BR-10 | Cancelling frees the slot; moving releases the old slot and claims the new one.                                     | —                                                               |

> **Design decisions to write in the report:** "within 7 days" means from the moment of booking; the 12-hour notice is measured against the currently booked start time; re-approval is needed after a change. The lecturer may interpret these differently, and being able to explain your choice is what counts.

## 3. Data

Reuse the existing `EnergyReservation` document (`Reservation.cs`). Fields: `id, prosumerNic, stationId, stationName, slotId, scheduledAt, status, qrToken, createdAt, updatedAt, completedAt, completedBy`. Consider adding:

- `createdBy` (user id of the staff member if booked on behalf) and `cancelledAt` / `cancelledBy`, which are useful for history and the report.
- `approvedAt` / `approvedBy`.
- Store `scheduledAt` as a copy of the slot's `startTime` (UTC), so rules and dashboards don't need a join.

Indexes (tell M1): `{prosumerNic, scheduledAt}`, `{status, scheduledAt}`, unique sparse `{qrToken}`.

**References between collections** (the DB design mark asks that these stay consistent): `prosumerNic → Users._id`, `stationId → SolarStationInfo._id`, `slotId → EnergyBookingSlots._id`.

### State machine

```
            create                 approve (operator)
   (none) ─────────▶ Pending ─────────────────────▶ Approved ──verify+complete (M4)──▶ Completed
                      │  ▲                             │
             cancel   │  └──── update (back to Pending, QR cleared)
                      ▼                                ▼ cancel
                  Cancelled ◀──────────────────────────┘
```

Put this diagram in the report. It is also your best viva answer.

## 4. API design

Controller: `ReservationsController` (thin: bind, authorize, call the service, return a DTO). Service: `IReservationService` / `ReservationService` (all rules). Repositories: extend the existing `ReservationRepository` and ask M2 for a `SlotRepository.TryClaimAsync` / `ReleaseAsync` (or add them yourself with M2's agreement).

| Method | Path                             | Roles                              | Body → Response                                                              |
| ------ | -------------------------------- | ---------------------------------- | ---------------------------------------------------------------------------- |
| POST   | `/api/reservations`              | Prosumer, Backoffice, GridOperator | `{stationId, slotId, prosumerNic?}` (nic only for staff) → `201` Reservation |
| PUT    | `/api/reservations/{id}`         | owner, staff                       | `{slotId}` (and `stationId` if changing station) → `200` Reservation         |
| POST   | `/api/reservations/{id}/cancel`  | owner, staff                       | `{reason?}` → `200` Reservation                                              |
| GET    | `/api/reservations/{id}`         | owner, staff                       | → `200` Reservation                                                          |
| POST   | `/api/reservations/{id}/approve` | GridOperator, Backoffice           | → `200` Reservation (with `qrToken`)                                         |

Return the contract `Reservation` shape every time. The mobile summary page is built from that response, so no second call is needed.

### Order of checks inside the service (explain this at the viva)

1. Load the booking (404) → 2. ownership / role (403) → 3. state (BR-6/7) → 4. time rules (BR-1/BR-2) → 5. account and station checks (BR-4/5) → 6. **atomic slot claim** (BR-3) → 7. write the reservation → 8. release the old slot on move/cancel.
   Doing the cheap read-only checks first means you never claim a slot and then have to roll back.

### Concurrency (BR-3)

Claim the slot with **one conditional update**: "set `isAvailable = false` where `_id = slotId` and `isAvailable = true`". If Mongo reports zero documents modified, someone else got it first → `SLOT_TAKEN`. Research: _MongoDB C# driver FindOneAndUpdate / UpdateOne with filter_. If the reservation insert then fails, release the slot again (a compensating action). Mention that multi-document transactions need a replica set, which is why you chose the conditional update.

### Time handling

All comparisons in **UTC** on the server (`DateTime.UtcNow`). The clients show Sri Lanka time (UTC+05:30). Write down the boundary behaviour: exactly 7 days = allowed; exactly 12 h = allowed; 11 h 59 m = rejected.

### QR token (BR-7)

Generate it with a **cryptographically secure** random generator (research: `RandomNumberGenerator` in .NET), at least 32 bytes, encoded URL-safe. The QR contains **only this opaque token**: no NIC, no name. M4's `verify-qr` looks it up and completes the booking. Agree the exact field name with M4 on Saturday.

## 5. Web (ASP.NET Core MVC + Bootstrap 5)

Controller `ReservationsController` in `SmartSolar.Web`, using `ApiClient`. No rules here: call the API and display the result or the error `message`.

| Page        | Contents                                                                                                                                                         |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Index**   | Upcoming bookings table (from M4's `GET /api/reservations?from=now`), status badges, actions: Edit / Cancel / Approve. Server errors shown in an alert banner. |
| **Create**  | Step form: prosumer NIC (lookup via M1's `/prosumers/{nic}`) → station dropdown (M2) → date → free-slot list (M2's `/stations/{id}/slots?date`) → confirm.       |
| **Edit**    | Current booking card + pick a new slot.                                                                                                                          |
| **Cancel**  | Confirmation page with the 12-hour rule explained; the server decides.                                                                                           |
| **Summary** | Shown after create/edit/cancel/approve: what changed, status, reference, and the QR token if approved.                                                           |

Visible to Backoffice and GridOperator (Approve to both). Match M4's style guide (cards, badge colours).

## 6. Android (Java, on M1's shell)

| Screen                                    | Behaviour                                                                                                                                                                                                                  |
| ----------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `BookSlotActivity`                        | Opened from the dashboard or from M2's map ("Book here" passes the station id). Pick a station → date (date picker limited to today…+7, **only a UX hint**) → free slots from the API → Confirm.                           |
| `MyBookingsActivity` (or reuse M4's list) | Item actions: Modify, Cancel, Show QR (only when `Approved`).                                                                                                                                                              |
| `ModifyBookingActivity`                   | Same picker, pre-filled; submit `PUT`.                                                                                                                                                                                     |
| Cancel                                    | Confirmation dialog → `POST /cancel`.                                                                                                                                                                                      |
| `BookingSummaryActivity`                  | **After every action**: an icon for created / updated / cancelled, station, local slot time, status, reference, "Back to dashboard".                                                                                       |
| `BookingQrActivity`                       | Renders `qrToken` as a QR bitmap (research: **ZXing core** `QRCodeWriter` / `BarcodeEncoder` from _zxing-android-embedded_; coordinate the library version with M4, who scans). Keeps the screen awake at full brightness. |

All errors come from the API `{code, message}` and are shown in M1's error dialog. **No date arithmetic decides anything on the phone.**

### SQLite

Add `my_reservations` to M1's `DbHelper` (bump the DB version, and handle it in `onUpgrade`). Upsert after every successful API response; `BookingQrActivity` reads from here when offline. Explain in the report: the server is the source of truth, and the local table is a read-only cache for the station visit.

## 7. Test cases (save screenshots as report evidence)

| #   | Case                                                                | Expected                                         |
| --- | ------------------------------------------------------------------- | ------------------------------------------------ |
| T1  | Book a slot 2 days ahead                                            | 201, `Pending`, slot now unavailable             |
| T2  | Book a slot 8 days ahead                                            | 409 `RULE_7DAYS`                                 |
| T3  | Book a slot in the past                                             | 409 `RULE_7DAYS`                                 |
| T4  | Two users book the same slot at the same time (two Swagger tabs)    | one 201, one 409 `SLOT_TAKEN`                    |
| T5  | Cancel a booking 13 h ahead                                         | 200, `Cancelled`, slot free again                |
| T6  | Cancel a booking 11 h ahead                                         | 409 `RULE_12H`                                   |
| T7  | Update an approved booking to a new slot                            | 200, `Pending`, `qrToken` cleared, old slot free |
| T8  | Prosumer A reads prosumer B's booking                               | 403                                              |
| T9  | Pending prosumer (`saman`) tries to book                            | 409 `ACCOUNT_NOT_ACTIVE`                         |
| T10 | Approve a `Cancelled` booking                                       | 409 `INVALID_STATE`                              |
| T11 | Approve a pending booking → the QR shows on the phone → M4 scans it | verified and `Completed`                         |
| T12 | Book at a deactivated station                                       | 400                                              |

Tip: for T5/T6, M1's seeder (or a Compass edit) lets you place a slot at exactly now + 11 h and now + 13 h.

## 8. Quality checklist (marks you can lose for free)

- [ ] Comment header on **every** `.cs` and `.java` file you create: file, description, **your name + IT number**, date.
- [ ] An inline comment at the **start of every method** saying what it does.
- [ ] Any adapted snippet (ZXing sample, Stack Overflow) cited above it with a URL.
- [ ] No `if (date …)` rule logic in `SmartSolar.Web` or Android.
- [ ] Every error path shows the server's message, and nothing crashes on 409/403/network loss.
- [ ] Commits from **your** account: small and descriptive, e.g. "Reject bookings more than 7 days ahead with RULE_7DAYS".
- [ ] All pages styled with Bootstrap 5, and the Android screens use the shared theme.

## 9. Roadmap (today → Wed)

| When                | Tasks                                                                                                                                                                                                         | Done when                              |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| **Sat 26, evening** | Create the branch. Draft the state machine + rule order in `docs/`. Service skeleton + `POST /reservations` with BR-1, 3, 4, 5, 9. Agree the QR field and library with M4 and the slot claim/release with M2. | T1–T4 pass in Swagger locally          |
| **Sun 27**          | PUT, cancel, GET, approve (BR-2, 6, 7, 8, 10). PR to `dev` **by 20:00**. Retest through IIS once M2 is live.                                                                                                  | T1–T10 pass via IIS; screenshots saved |
| **Mon 28**          | Web Index, Create, Edit, Cancel, Summary. Start Android `BookSlotActivity` on M1's shell.                                                                                                                     | Web flow end to end                    |
| **Tue 29**          | Android modify/cancel, summary, QR screen, SQLite cache. 18:00 team LAN test (T11 with M4).                                                                                                                   | Real phone, all flows                  |
| **Wed 30 (AM)**     | Bug fixes, final screenshots.                                                                                                                                                                                 | Feature freeze 12:00                   |
| **Wed 30 (PM)**     | Report: DFD (level 0 + 1), your source code as text, test table, references list, contribution + AI disclosure + reflection, challenges. 70 s video segment.                                                  | Submitted by 18:00                     |

## 10. Report pieces you own

- **DFD level 0** (the system with Prosumer, Grid Operator, Backoffice as external entities) and **level 1** (processes: Manage Accounts, Manage Nodes, Manage Reservations, Verify & Complete; stores: the four collections, plus SQLite on the device).
- The state machine diagram + rule table + test results table from this guide.
- A consolidated reference list (IEEE or APA, the same style throughout).

## 11. Viva questions to rehearse

1. Walk through what happens, layer by layer, when a prosumer taps _Confirm booking_.
2. Where is the 12-hour rule, and why not in the app?
3. How do you stop two people booking the same slot?
4. Why does an approved booking go back to Pending after a change?
5. What's inside the QR, and why can't someone forge one?
6. What's in SQLite on the phone, and what happens when it disagrees with the server?
7. Show me how you would change the 7-day window to 5 days. (Answer: one constant in the service, nowhere else.)
8. Which parts did AI help plan, and what did you change from that plan?
