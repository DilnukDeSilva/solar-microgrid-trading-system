# Member 2: Microgrid Nodes, Slots & Maps (+ IIS hosting)

Branch: `feature/m2-nodes-maps-iis` · Module owner for solar grid hubs (stations), their battery slots and schedules, the nearby-stations map, and the IIS deployment.

## Marks you lead
- **Individual:** Microgrid node management (5), **Show nearby stations on the map (5)**, **Google Maps API integration (3)**, SQLite (shared), Web↔API (2), Mobile↔API (2).
- **Group:** **Hosting the Web API on IIS (4)**, reproducible deployment (Documentation), use case diagram.

## Scope

### IIS + LAN (first, done by **Sun 27, 12:00**)
Follow `docs/DEPLOYMENT.md` on a Windows PC: .NET **10** Hosting Bundle, publish, IIS site with the app pool set to *No Managed Code*, config through `appsettings.Production.json` or environment variables (never in Git), static IP, firewall inbound rule, MongoDB reachable. Host the **web app** as a second site. Announce the base URL. Update `DEPLOYMENT.md` with what actually happened, with screenshots.

### API (`StationsController`, `SlotsController`, `StationService`, `SlotService`)
| Endpoint | Role | Rules |
|---|---|---|
| `GET /api/stations`, `GET /api/stations/{id}` | any logged-in role | |
| `POST /api/stations`, `PUT /api/stations/{id}` | Backoffice | name required; lat −90..90, lng −180..180; capacity > 0; battery slots ≥ 1; schedule open < close |
| `POST /api/stations/{id}/deactivate` | Backoffice | **409 `STATION_HAS_RESERVATIONS`** if any future `Pending`/`Approved` reservation exists; `POST /{id}/activate` to undo |
| `GET /api/stations/{id}/slots?date` | any | |
| `POST /api/stations/{id}/slots`, `PUT /api/slots/{id}`, `DELETE /api/slots/{id}` | Backoffice, GridOperator | slot inside the station schedule; no overlapping slots beyond `batterySlotsTotal`; cannot delete or close a slot that has an active reservation |
| `PUT /api/slots/{id}/availability` | GridOperator | the operator updates battery slot availability (from the project scenario) |
| `GET /api/stations/nearby?lat&lng&radiusKm` | any | server computes the distance (haversine), returns only `Active` stations sorted by distance, with the free slot count |

Add the repository write methods you need to `StationRepository` / `SlotRepository`, and a 2dsphere or lat/lng index if you use a Mongo geo query.

### Web (React + Tailwind CSS, in `WebApp/smartsolar-ui`)
Node list (status badges, search) · create/edit with GPS fields (optionally a small map preview) · deactivate with the server's 409 message shown clearly · schedule editor · slot management per node with an availability toggle for operators.

### Android
- **Nearby stations map:** Google Maps SDK, location permission flow (and a fallback to Colombo if denied), markers from `/stations/nearby`, tap a marker to open a bottom sheet with name, capacity, free slots, schedule and a **"Book here"** button that opens M3's booking screen with the station id.
- **Station list** screen (a list alternative to the map).
- Maps API key kept in `local.properties` / manifest placeholder, **not committed**.

### SQLite
`stations_cache(id PK, name, lat, lng, capacity_kwh, battery_slots, status, synced_at)`: the map and list show cached stations when offline, and it refreshes on open. This is "reference data persists in SQLite" from the marking scheme.

## Roadmap
| When | Do |
|---|---|
| Sat 26 PM | IIS setup on Windows. Station service + endpoints. |
| Sun 27 | **12:00: IIS live and announced.** Slots + availability + nearby endpoints merged by 20:00. |
| Mon 28 | Web node, schedule and slot pages. Get the Maps key working in the Android shell. |
| Tue 29 | Android map, bottom sheet, list, SQLite cache. 18:00 team LAN test is on your machine. |
| Wed 30 | Deployment section + use case diagram, screenshots, your code in the report, video segment. |

## Definition of done
Deactivate a node with and without reservations (both results captured); overlapping slots rejected; markers come from the API, not a fixed list; a real phone reaches IIS by IP.

## Viva prep
What the ASP.NET Core Module and the Hosting Bundle do, why *No Managed Code*, how secrets reach IIS, how the haversine distance works, how the map gets its data, and why deactivation is checked on the server.
