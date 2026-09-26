# Member 1 (Dilnuk): Identity & Accounts

Branch: `feature/m1-identity` · Module owner for users, prosumers, login, the Android app shell and SQLite session storage.

## Marks you lead
- **Individual:** Web login + role-based access (4) ✅, Web user management (4) ✅, **Mobile authentication & account management (9)**, SQLite local persistence (3, shared), Web↔API (2), Mobile↔API (2).
- **Group:** MongoDB connection & layering (part of Service Architecture), Database design (4), Client build & architecture (the Android shell and web skeleton).

## Already done on `dev`
API foundation, Mongo + seed + indexes, JWT, staff Users API, web login / role redirect / user pages, deployment doc. 

## Remaining scope

### API (your own controllers and services; reuse `UserRepository`)
| Endpoint | Role | Rules |
|---|---|---|
| `POST /api/auth/register` | anon | NIC format valid (old 9 digits + V/X, or new 12 digits); NIC unique → `NIC_EXISTS`; username unique; password ≥ 8; creates `Prosumer` with status **`Pending`** |
| `GET /api/me`, `PUT /api/me` | Prosumer | NIC and role cannot be changed; email and phone validated |
| `POST /api/me/request-deactivation` | Prosumer | **Decision to document:** sets status `Deactivated` immediately, logs the user out; only Backoffice can reactivate. |
| `GET /api/prosumers?status&q` | Backoffice, GridOperator | search by NIC, name, phone |
| `GET/POST/PUT /api/prosumers/{nic}` | Backoffice | staff can create prosumers directly as `Active` |
| `POST /api/prosumers/{nic}/deactivate` | Backoffice | |
| `POST /api/prosumers/{nic}/reactivate` | **Backoffice only** | GridOperator gets 403 |
| `GET /api/prosumers/pending`, `POST /api/prosumers/{nic}/activate` | Backoffice | only `Pending` → `Active` |

### Web (MVC + Bootstrap 5)
- **Prosumers** page: list with search, create, edit, deactivate; **Reactivate** button visible only to Backoffice.
- **Pending activations** page: table of `Pending` prosumers with Activate / Reject, and a badge count in the navbar (2 marks on its own).

### Android: the app shell (merge by **Sun 27, 12:00**, the others depend on it)
- Project setup: Java, min SDK 24, package layout `data/local`, `data/remote`, `ui/<module>`, `util`.
- `ApiClient` (base URL from a settings screen or `BuildConfig`, bearer token on every call, parses `{code, message}` errors), `SessionManager`, `DbHelper extends SQLiteOpenHelper` (one DB for the app, **each module adds its own table** via a version bump), `BaseActivity` (loading spinner + error dialog), shared colours / styles.
- Role routing after login: Prosumer → prosumer home (M4 fills in the dashboard), GridOperator → operator home (M4 fills in the operator tools).

### Android: your screens
Splash → Login (username or NIC) → role home · Register (NIC as the primary key, shows "awaiting activation") · Profile view/edit · Request deactivation (confirm dialog) · Logout · Friendly messages for `ACCOUNT_NOT_ACTIVE` and `NIC_EXISTS`.

### SQLite (your tables)
- `session(token, expires_at, role, user_id, nic)`: keeps the user logged in after an app restart; cleared on logout or a 401.
- `user_profile(nic PK, username, full_name, email, phone, status, synced_at)`: the profile screen shows cached data first, then refreshes from the API.

## Roadmap
| When | Do |
|---|---|
| Sat 26 PM | Android shell: project, ApiClient, SessionManager, DbHelper, role routing. Register/me API. |
| Sun 27 | **12:00: shell merged, announced.** Prosumer admin API + pending/activate/reactivate. Help M2 remotely with IIS. |
| Mon 28 | Web prosumer pages + pending activation page. Android login, register, profile, deactivation. |
| Tue 29 | SQLite session / profile polish, error handling, real phone test via IIS. Report: high-level diagram, DB design. |
| Wed 30 | Screenshots, your source code into the report, contribution + AI reflection, video segment (70 s). |

## Definition of done
Every rule tested with a boundary case (duplicate NIC, invalid NIC, Pending login, GridOperator trying to reactivate → 403). The app survives a restart logged in. File headers carry your name and IT number.

## Viva prep
JWT and where the key lives; how `ActiveAccountGuard` rejects deactivated tokens; why NIC is the `_id`; what the app stores in SQLite versus MongoDB and why; how role routing works on both clients.
