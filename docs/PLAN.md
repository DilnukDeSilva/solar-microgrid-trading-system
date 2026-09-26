# Smart Solar Microgrid Trading System: Rebalanced Plan (v2, 26 Sep 2026)

This replaces the work split in PLAN v1. The architecture, the collections and `API-CONTRACT.md` stay the same. What changes is **who owns what**. Each member now owns one **module end to end**: its API endpoints and business rules, its web pages, its Android screens, and one local SQLite use. So each person can demo their module on both clients, write their own report section, and defend all of it at the viva.


## 1. Architecture (unchanged)

```
 Web App (ASP.NET Core MVC + Bootstrap 5)        Android App (Java, pure native)
   UI only: no DB, no business rules              UI + SQLite (session, profile, cached reference data)
              \                                   /
               \______ REST / JSON over LAN _____/
                              |
              C# ASP.NET Core Web API on Windows IIS (FAT service)
                 Controllers (thin) -> Services (ALL rules) -> Repositories
                              |
                  MongoDB: Users, SolarStationInfo, EnergyBookingSlots, EnergyReservation
```

**Android language: Java** (one language for the whole app, and it matches the lecture material). If the whole team prefers Kotlin, decide **today** and stick to it.

## 2. The four modules

| # | Module | API (service + controller) | Web (MVC + Bootstrap 5) | Android | SQLite use |
|---|---|---|---|---|---|
| **M1** | **Identity & Accounts** | login ✅, staff users ✅, prosumer register, `/me`, prosumer admin, pending/activate/reactivate | login + role redirect ✅, staff users ✅, **prosumer management**, **pending activation view** | **app shell** (API client, session, DB helper, role routing), login → role home, register (NIC), edit profile, request deactivation | `session`, `user_profile` |
| **M2** | **Microgrid Nodes, Slots & Maps** (+ **IIS hosting**) | stations CRUD + deactivate rule, schedules, slots CRUD / availability, `stations/nearby` | node list/create/edit/deactivate (GPS, capacity, battery slots, schedule), slot management | **nearby stations map** (Google Maps SDK), station details sheet, station list | `stations_cache` (reference data) |
| **M3** | **Energy Reservations & QR Dispatch** | create/update/cancel/get, approve → QR token; 7-day, 12-hour, slot, active-account rules | reservation management (create for a prosumer, edit, cancel, approve) | book (station → slot → confirm), modify, cancel, **summary page after each action**, **show QR** for approved bookings | `my_reservations` (offline booking + QR view) |
| **M4** | **Booking Views, Dashboards & Operator Verification** | reservation list with filters, pending queue, dashboard counts, verify QR, complete job | **Home (index) dashboard** with live counts per role, **booking monitor** with filters and history | prosumer dashboard, current / pending / history tabs, search filter, **operator mode** (scan QR → verify → finalise) | `booking_history_cache`, last dashboard snapshot |

✅ = already done by Member 1 on `dev`.

## 3. Coverage check: every spec item has one owner

| Assignment spec item | Owner |
|---|---|
| Web users with two roles, Backoffice-only admin functions | M1 |
| Prosumer create/update/deactivate by NIC; reactivation only by Backoffice | M1 |
| Microgrid nodes: GPS, capacity (kW/h), battery slots, schedules, deactivation blocked by active reservations | M2 |
| Reservations: within 7 days; update/cancel need 12 h notice | M3 |
| Web UI in Bootstrap 5, responsive | all (M4 owns the Home page and shared style guide) |
| Pure native Android + SQLite, no cross-platform framework | all (M1 builds the shell) |
| Prosumer register (NIC), edit profile, request deactivation | M1 |
| Reserve / modify / cancel slots; QR once approved | M3 |
| Dashboard counts, bookings, history, pending, search | M4 |
| Nearby grid nodes on Google Maps | M2 |
| Operator logs in, scans QR, verifies with server, finalises | M4 (M1 routes the operator to the operator home) |
| FAT service, C# Web API on IIS, MongoDB | M1 (architecture, DB) + M2 (IIS) |
| Clients talk only through REST | all |

## 4. Marks map (why this is balanced)

**Individual (Table 2, 65 marks per student)**

| Criterion (marks) | M1 | M2 | M3 | M4 |
|---|---|---|---|---|
| Web features (18): login/roles 4 · users 4 · nodes 5 · slot booking 5 | 8 | 5 | 5 | Home + monitor (supports) |
| Mobile auth & account (9) | 9 | | | |
| Reservation workflow (9) | | | 9 | |
| Booking views & dashboards (10) | | | | 10 |
| Operator verification & map (7): QR done 2 · map 5 | | 5 | QR generation | 2 |
| Service integration (12): web↔API 2 · mobile↔API 2 · SQLite 3 · Maps 3 · QR scan 2 | web + mobile + SQLite | web + mobile + SQLite + Maps 3 | web + mobile + SQLite | web + mobile + SQLite + QR scan 2 |
| **Approx. marks led** | **~21** (8 of them already done) | **~15** + IIS | **~16** + QR | **~16** + Home |

M1 leads more marks on paper, but the web half is already built. The **remaining** work is about equal for all four.

**Group (Table 1, 35 marks)**

| Criterion | Lead | Everyone |
|---|---|---|
| Service architecture & API design (8): IIS 4 · MongoDB 4 | M2 (IIS), M1 (Mongo, layering) | keep all rules in services |
| Database design (4) | M1 | add your module's indexes and seed rows |
| Client build & architecture (12) | M1 (Android shell, web skeleton) | no logic in clients, graceful errors |
| UI/UX (6): mobile 2 · web 2 · home 1 · completeness 1 | M4 (Home + style guide) | finish every page in your module |
| Documentation & deployment (5) | see §7 | own section + contribution + challenges |

> **Ask the lecturer on Monday:** is each student marked on **all six** Table 2 criteria, or on their own module? If it's all six, each member adds one small screen outside their module. Suggested pairs: M1 ↔ M3 (M1 adds the web "cancel on behalf" button, M3 adds the mobile profile screen polish) and M2 ↔ M4 (M2 adds the web operations dashboard tile, M4 adds the station details bottom sheet).

### Changes to the owner column in `API-CONTRACT.md` (one PR, all four approve)
| Endpoints | Old owner | New owner |
|---|---|---|
| `/prosumers*` (admin), `/auth/register`, `/me*`, `/prosumers/pending`, `/prosumers/{nic}/activate` | M2 / M3 | **M1** |
| `/stations*`, `/slots*`, **`/stations/nearby`** | M2 / M4 | **M2** |
| `/reservations` create/update/cancel/get, **`/reservations/{id}/approve`** | M2 / M4 | **M3** |
| `/reservations` list + filters, `/reservations/pending`, `/dashboard/*`, `verify-qr`, `complete` | M4 | **M4** (+ new `GET /dashboard/operations` for the web Home) |
| New error codes: `INVALID_STATE` (M3), `SLOT_OVERLAP` (M2) | — | add to the contract |

## 5. Dependencies and hand-offs (keep these times)

| Hand-off | From → to | Due |
|---|---|---|
| API reachable on IIS over the LAN + seed logins announced | M2 → all | **Sun 27, 12:00** |
| Android shell merged to `dev` (API client, session, DB helper, role home, base styles) | M1 → M2, M3, M4 | **Sun 27, 12:00** |
| Reservation create/update/cancel/approve endpoints merged | M3 → M4 | **Sun 27, 20:00** |
| Reservation list/filter endpoint merged | M4 → M3 (web list) | **Sun 27, 20:00** |
| Stations + slots + nearby endpoints merged | M2 → M3 (booking picker), M4 | **Sun 27, 20:00** |
| Contract changes | anyone → all | via PR only, all four approve |

Until a hand-off lands, work against **Swagger on Member 1's dev host** or the JSON in `docs/mock/`. Never wait idle.

## 6. Team schedule (today → deadline Wed 30, 23:59; we submit by **18:00**)

| Day | Everyone |
|---|---|
| **Sat 26 (PM)** | Agree this plan and the contract changes. Create the branches (§8). M2 starts IIS. M1 starts the Android shell. M3 and M4 build their API services. |
| **Sun 27** | Morning: hand-offs at 12:00. All module APIs done by night with Swagger evidence of each rule. |
| **Mon 28** | Web pages for every module done. Android screens start. Ask the lecturer about §4. |
| **Tue 29** | Android screens done. Full LAN run on a **real phone through IIS** at 18:00. Freeze features at 22:00. |
| **Wed 30** | Bug fixes until 12:00, then screenshots, report, README, video. Zip `ITxxxxxxxx.zip`, **submit by 18:00**. |

## 7. Report and submission split

| Item (from the brief) | Owner |
|---|---|
| High-level diagram, architecture & FAT service explanation | M1 |
| Database design (collections, fields, references, indexes) | M1 (each member supplies their fields) |
| Use case diagram | M2 |
| DFD (level 0 + level 1) | M3 |
| IIS hosting & deployment steps (reproducible), smoke-test evidence | M2 |
| Screenshots of **all** UIs (unique, consistent device and browser) | each owner for their module; M4 compiles |
| Source code pasted as text | each owner for their module |
| References (consistent style, e.g. IEEE) | M3 collects, everyone adds |
| Git repository link, README, per-member contributions with commit links | M4 |
| Video ≤ 5 min (about 70 s per module), link in README | M4 edits, everyone records their part |
| Individual contribution + AI planning disclosure + reflection | each member |
| Challenges | each member (one paragraph each, genuine) |
| Screenshot of the main opening screen in the zip | M1 (login / splash) |

## 8. Git

- Branches: `feature/m1-identity`, `feature/m2-nodes-maps-iis`, `feature/m3-reservations`, `feature/m4-dashboards-operator`. PR into `dev`, and `dev` into `main` on Wednesday.
- Commit small and often, **from your own GitHub account**, with messages that say what and why.
- Every `.cs` / `.java` file: comment header block with **your name and IT number** (not "Member 1"). An inline comment at the start of every method. Uncommented code is not marked.
- Cite any snippet you adapted (URL + what you changed) in a comment above it.

## 9. Definition of done (every module)

- Every rule in your module returns the contract error `{code, message}` from the API, and both clients show that message. The clients never decide rules themselves.
- Your web pages and Android screens are finished, styled and responsive. No dead buttons.
- Tested through **IIS** from a **real phone**, not just localhost.
- Boundary test evidence saved for the report (screenshots of Swagger or app messages).
- You can open any file in your module at the viva and explain or change it.
