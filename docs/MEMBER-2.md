# Member 2: Web Operations: Prosumers, Microgrid Nodes, Slots, Reservations

## Marks you own
- Individual: Microgrid Node Management (5), Slot Booking Management (5), share of Web app features (18), Web ↔ API (2)
- Group: UI/UX web interfaces + Home page (Bootstrap 5)

## Blocked until
- M1's Milestone 0 (login, DB, base URL). Until then, work from the contract with mock JSON and design the pages.
- M1's web skeleton + API client (about Day 4). Before it lands, build the API side, which does not need it.

## Milestone 1: API first (Day 2–5)
Write your endpoints in your own controller and service files.
1. **Stations**: create (name, GPS, capacity kWh, battery slots), update, list, get, deactivate. Deactivate must return `409 STATION_HAS_RESERVATIONS` if any Pending/Approved future reservation exists.
2. **Slots**: create/update/delete slots for a station and update the schedule.
3. **Prosumers admin**: create, update, list/search, deactivate, and reactivate. Reactivation is **Backoffice only**, and a GridOperator gets 403.
4. **Reservations core** (`POST`, `PUT`, `cancel`, `GET by id`) with the rules below. **M3 needs these by Day 5**, so build them first.
   - 7-day window and not in the past → `RULE_7DAYS`
   - update/cancel at least 12 h before `scheduledAt` → `RULE_12H`
   - slot must be free → `SLOT_TAKEN`; booking a slot marks it unavailable and cancelling frees it
   - prosumer must be `Active` → `ACCOUNT_NOT_ACTIVE`
   Rules live in **one service class** that both web and mobile reach through the API.

## Milestone 2: web pages (Day 4–8)
- Nodes: list, create (with GPS fields), edit, deactivate with the server's error shown clearly.
- Slots: manage per node.
- Prosumers: list/search, create, edit, deactivate, reactivate (button shown to Backoffice only).
- Reservations: list, create, edit, cancel, showing the server's rule messages.
- Home (index) page: polished landing with role-aware quick links. UI consistency across the whole web app: Bootstrap 5, responsive, no page left unfinished.

## Depends on / blocks
- Depends on: M1 (auth, DB, web skeleton).
- Blocks: M3 (reservation endpoints for the mobile booking flow) and M4 (approval, history and QR need reservations to exist). Ship the reservation endpoints early.

## Definition of done
- Try every rule with a boundary case: exactly 7 days, 6 h 59 m vs 12 h 01 m, double booking, deactivating a station with and without reservations. Save these as your test evidence for the report.
- Comment header on every `.cs` file, inline comment at the start of every method.
- The web app contains no rule logic, only a call and a display of the response.

## Viva prep: be able to explain
Why rules sit in the service layer and not the controller or UI, how you compare times in UTC, how double booking is prevented under concurrent requests, how the 409 error reaches the screen.
