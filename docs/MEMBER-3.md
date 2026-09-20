# Member 3: Android App Foundation: Account, SQLite, Booking Workflow, Pending Activation

## Marks you own
- Individual: Mobile Authentication & Account Management (9), Reservation Workflow (9), SQLite local persistence (3), Mobile ↔ API (2)
- Group: Mobile interfaces (share with M4)

## Blocked until
- M1's Milestone 0 for real login. Until then, build UI and SQLite against mock JSON from `API-CONTRACT.md`.
- M2's reservation endpoints (target Day 5) for the booking screens. You can build the screens and local layer earlier.

## Milestone 0: unblock M4 (Day 1–3): **M4 depends on this**
- Pure native Android project (Java or Kotlin, **no cross-platform, no Flutter/React Native/Xamarin**).
- Package structure agreed with M4: `data/local` (SQLite), `data/remote` (API client), `ui/`, `util/`.
- Shared plumbing: API client with base URL setting and bearer-token interceptor, the error-body parser, a session store, and a role-aware navigation shell (Prosumer home vs Operator home tabs). M4 plugs their screens into this shell.
- **SQLite** via `SQLiteOpenHelper` (confirm with your lecturer if you want to use Room): tables for the logged-in user/session and reference data (e.g. cached stations). State clearly in the report what is stored locally and why.
- Push it to `dev` by **end of Day 3**.

## Milestone 1: account features (Day 3–6)
- Register (NIC as primary key, client-side format check only for UX; the server validates) → `POST /auth/register`. Show "awaiting activation".
- Login → role-based home (Prosumer vs GridOperator). Handle `ACCOUNT_NOT_ACTIVE`.
- Edit profile (`PUT /me`), request deactivation.
- API side (your own controllers): `/auth/register` (creates `Pending`), `/me`, `/me/request-deactivation`, `/prosumers/pending`, `/prosumers/{nic}/activate`.
- **Web "pending activation" view** (Backoffice page listing pending prosumers with Activate button). Add it into M1's web skeleton after Day 4.

## Milestone 2: booking workflow (Day 5–8)
- Create booking: pick a station, pick a free slot, submit.
- Update and cancel own booking. The server enforces 7-day and 12-hour rules, so display the server message.
- **Summary screen after each action** (created / updated / cancelled): station, slot, time, status.

## Depends on / blocks
- Depends on: M1 (login, server), M2 (reservation endpoints).
- Blocks: M4 (navigation shell, API client, SQLite helper, session).

## Definition of done
- Registration, login and profile edit work end to end against the hosted API on a real phone over LAN.
- Nothing in the app decides business rules (no date checks except for UX hints).
- Comment header and inline comments on all your files. Cite any snippet you didn't write.

## Viva prep: be able to explain
What lives in SQLite vs MongoDB and why, how the token is attached to calls, how the session survives an app restart, what happens offline, how you show the 12-hour rule error.
