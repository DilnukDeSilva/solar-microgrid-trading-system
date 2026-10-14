# Smart Solar Microgrid Trading System (SE4040 EAD Assignment 1)

Client-server system: C# Web API on IIS + MongoDB, an ASP.NET Core MVC web app (Bootstrap 5), and a native Android app (Java) with SQLite.

- **Git repository:** https://github.com/DilnukDeSilva/solar-microgrid-trading-system
- **Demo video (max 5 min):** https://mysliit-my.sharepoint.com/:f:/g/personal/it22635266_my_sliit_lk/IgD29zEAVDS7QogBZO2GQd36AeI5sxG1P0mV0CeBLBCt84c?e=pFIdua

| Folder | Content |
|---|---|
| `WebService/` | C# Web API (FAT service: every business rule lives here) + MongoDB |
| `WebApp/` | ASP.NET Core MVC web app for Backoffice and Grid Operators (UI only, calls the API) |
| `MobileApp/` | Native Android app (Java) for prosumers and grid operators, with SQLite |
| `docs/` | Plan, deployment notes, and mock API payloads |

Start with `docs/PLAN.md` and `docs/DEPLOYMENT.md`. Mock API samples live under `docs/mock/`.

## Individual contributions

Each member owned one module end to end: its API endpoints and rules, its web pages and its Android screens.

| Member | Module | What they built | Commits |
|---|---|---|---|
| **DE SILVA R K D H (IT22001252)** | Identity & accounts | Solution skeleton, MongoDB connection, collections, indexes and seed data, JWT login with role-based access, staff users API and web pages, prosumer registration, profile, deactivation and Backoffice activation (API and web), Android app shell (API client, session, SQLite `session` / `user_profile`, role routing), login, register, profile and deactivation screens, shared web theme | [commits](https://github.com/DilnukDeSilva/solar-microgrid-trading-system/commits/dev?author=DilnukDeSilva) |
| **Mohamed Asath (IT22633422)** | Microgrid nodes, slots & maps | Stations with GPS, capacity, battery slots and weekly schedule, deactivation blocked by future bookings, slot management with schedule and battery overlap rules, nearby stations API, web stations and slots pages, Android Google Map and station list with SQLite station cache, IIS hosting | [pull request #13](https://github.com/DilnukDeSilva/solar-microgrid-trading-system/pull/13) |
| **Janukshan S (IT22635266)** | Energy reservations & QR dispatch | Reservation create, update, cancel, get and approve with the 7-day and 12-hour rules, atomic slot claim (`SLOT_TAKEN`), active prosumer and station checks, status rules and a random single-use QR token, bookable station and free slot endpoints, web reservation management (list, book on behalf of a prosumer, edit, cancel, approve, summary), Android booking, change slot, cancel, summary after each action and QR screen, SQLite `my_reservations` so the QR opens offline | [commits](https://github.com/DilnukDeSilva/solar-microgrid-trading-system/commits/dev?author=JanukshanS) |
| **Herath D M S T (IT22639776)** | Booking views, dashboards & operator verification | Reservation list with filters and paging, pending approval queue, prosumer and operations dashboards, QR verify and job completion, web Home dashboards and booking monitor, Android prosumer dashboard, My bookings tabs and search with SQLite cache, operator home with camera QR scanning and job finalising | [commits](https://github.com/DilnukDeSilva/solar-microgrid-trading-system/commits/dev?author=samudithTharindaka) |

## Run locally

The JWT key is not in Git. Copy `WebService/SmartSolar.Api/appsettings.Development.json.example` to `appsettings.Development.json` (or use `dotnet user-secrets`), then:

```bash
dotnet run --project "WebService/SmartSolar.Api/SmartSolar.Api.csproj" --launch-profile lan
dotnet run --project "WebApp/SmartSolar.Web/SmartSolar.Web.csproj"
```

API: `http://localhost:5080/swagger` · Web: `http://localhost:5081` · Health: `http://localhost:5080/health`

Android: open `MobileApp` in Android Studio (Gradle JDK 17). On a real phone, set **API settings** on the login screen to `http://<server-ip>:5080/api/`. The Google Maps key goes in `MobileApp/local.properties` as `MAPS_API_KEY=...` (not in Git).

Seed logins: `admin / Admin@123` (Backoffice), `operator1 / Oper@123` (GridOperator), `nimal / Solar@123` (Active prosumer).

## Git workflow
- `main`: submission-ready only. `dev`: integration branch.
- Each member worked on their own branch and merged to `dev` by pull request.

| Branch | Member |
|---|---|
| `feature/service-auth-iis` | DE SILVA R K D H |
| `feature/m2-nodes-maps-iis` | Mohamed Asath |
| `feature/reservations` | Janukshan S |
| `feature/samu-dashboard-operator` | Herath D M S T |
