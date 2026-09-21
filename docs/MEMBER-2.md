# Member 2: IIS/LAN Deployment, Web Operations: Prosumers, Microgrid Nodes, Slots, Reservations

## Marks you own
- Individual: Microgrid Node Management (5), Slot Booking Management (5), share of Web app features (18), Web ↔ API (2)
- Group: UI/UX web interfaces + Home page (Bootstrap 5), **Hosting the Web API on IIS (part of Service Architecture, 4 marks)**, reproducible deployment (Documentation & Deployment)

## Blocked until
- M1's Milestone 0 (login, DB, base URL). Until then, work from the contract with mock JSON and design the pages.
- M1's web skeleton + API client (about Day 4). Before it lands, build the API side, which does not need it.

## Milestone 0: IIS deployment (moved from Member 1; do this first)
Member 1 develops on a Mac, and IIS runs only on Windows, so you own the hosting. **Do this as soon as M1's pull request is merged into `dev`, ideally within half a day.** M3 and M4 need a reachable server, and the group is blocked on it.

You need a Windows PC or laptop (yours, a lab machine, or a VM). Follow `docs/DEPLOYMENT.md`, which M1 wrote from the Mac side, and fix it wherever the real Windows run differs.

1. Install MongoDB Community on the Windows machine (or point the API at a reachable MongoDB) and confirm it runs.
2. Enable IIS. Install the **.NET Hosting Bundle** that matches the API's target framework (`net10.0`), then restart IIS.
3. Publish the API from `WebService/SmartSolar.Api` (framework-dependent). The publish profile `IISFolder.pubxml` and `web.config` are already in the repo.
4. Create an IIS site for the publish folder with the app pool set to **No Managed Code**. Give the app pool identity read access to the folder.
5. Configuration must not be in Git. Copy `appsettings.Production.json.example` to `appsettings.Production.json` on the server (or set environment variables) with the Mongo connection string and a real JWT key (32+ characters).
6. Give the PC a **static LAN IP** (or a DHCP reservation), bind the site to all IPs on the chosen port, and add a **Windows Firewall inbound rule** for that port.
7. Test: `/health` on the PC, then from a phone on the same Wi-Fi, then `POST /api/auth/login` from Swagger through IIS. Seed data loads on first start, so MongoDB must be reachable then.
8. Publish the web app (`WebApp/SmartSolar.Web`) as a second IIS site or app, with `Api:BaseUrl` set to the API's LAN address.
9. Fix the common errors: 500.19 (missing hosting bundle or module), 502.5 (app crashed at startup, check the `logs` folder), Mongo unreachable from the app pool, firewall blocking the port.
10. Message the group with the base URL, Swagger URL and seed logins (see the announcement text in `docs/DEPLOYMENT.md`).
11. Write and run the **smoke-test checklist**: health, login for each role, 401 and 403 checks, create a staff user in the web app, and a real phone calling the API over the LAN. Keep screenshots for the report.

M1 helps remotely (screen share, config questions, API startup errors). If an error is in the API code, ask M1 to fix it on their branch.

Done when: a phone on the same Wi-Fi opens `http://<pc-ip>:<port>/health`, and M3 and M4 confirm they can log in.

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
- Depends on: M1 (auth, DB, web skeleton, and the merged pull request for the IIS deployment).
- Blocks: M3 and M4 (a reachable server for real login and LAN tests), plus M3 (reservation endpoints for the mobile booking flow) and M4 (approval, history and QR need reservations to exist). Ship the reservation endpoints early.

## Definition of done
- Try every rule with a boundary case: exactly 7 days, 6 h 59 m vs 12 h 01 m, double booking, deactivating a station with and without reservations. Save these as your test evidence for the report.
- Comment header on every `.cs` file, inline comment at the start of every method.
- The web app contains no rule logic, only a call and a display of the response.

## Viva prep: be able to explain
How the ASP.NET Core Module and app pool run the API, why "No Managed Code", what the Hosting Bundle does, and how configuration and secrets reach the app on IIS. Also: why rules sit in the service layer and not the controller or UI, how you compare times in UTC, how double booking is prevented under concurrent requests, how the 409 error reaches the screen.

## Report
You write the **deployment** section of the report (steps, screenshots of IIS, firewall rule, phone test, and the smoke-test results). M1 reviews it.
