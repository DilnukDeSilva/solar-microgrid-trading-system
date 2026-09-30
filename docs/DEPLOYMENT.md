# Deployment: IIS / LAN

This is the reproducible deployment record for the FAT Web API and the web app.

- **Windows IIS (assignment hosting, owned by Member 2):** follow "Windows IIS host" below. This is what the marks are for.
- **Development host (Member 1's Mac):** used only until the Windows machine is ready, so M2–M4 are not blocked. Kestrel is bound to all interfaces on port 5080.

Every `<host-ip>` below is a placeholder. Replace it with the real LAN IPv4 of whichever machine is hosting (the Windows PC once IIS is live).

Official docs used (cited for the viva / report):
- [Host ASP.NET Core on Windows with IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [ASP.NET Core Module](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/aspnet-core-module)

## Copy-paste group announcement (fill in `<host-ip>` first)

```
Smart Solar API is up on the LAN.

Base URL:     http://<host-ip>:5080/api
Health:       http://<host-ip>:5080/health
Swagger:      http://<host-ip>:5080/swagger

Android emulator uses 10.0.2.2 instead of <host-ip>
(example: http://10.0.2.2:5080/api).

Seed logins (passwords are hashed in Mongo, these are plaintext for testers only):
- Backoffice:     admin / Admin@123
- GridOperator:   operator1 / Oper@123
- Prosumer:       nimal  OR  NIC 200012345678 / Solar@123
- Pending:        saman  OR  NIC 199912345678 / Solar@123
                  (login returns 409 ACCOUNT_NOT_ACTIVE — expected)

Please confirm you can open http://<host-ip>:5080/health and log in as admin.
(/api/health returns the same JSON if you prefix everything with /api.)
If <host-ip> times out: check that we are on the same Wi-Fi, that the host PC is awake, and that the firewall rule for the port exists.
```

When IIS is live, use the Windows PC's static IP as `<host-ip>` and send the same message again. Add the web app URL `http://<host-ip>:5081` once it is hosted (step 10).

---

## Development host (Member 1's Mac): temporary, until Windows/IIS is ready

**IIS cannot be installed on a Mac.** Kestrel is bound to all interfaces so a phone on the same Wi-Fi can call the API. Clients never talk to Mongo; only the API process does.

| Item | Value |
|---|---|
| LAN IPv4 (Wi-Fi `en0`) | find it with `ipconfig getifaddr en0` (call it `<mac-ip>`) |
| Port | `5080` |
| MongoDB | `mongodb://localhost:27017` / database `SmartSolar` |
| macOS firewall | Off (required for inbound 5080) |

First-time local setup (JWT key is **not** in Git):

```bash
cp WebService/SmartSolar.Api/appsettings.Development.json.example \
   WebService/SmartSolar.Api/appsettings.Development.json
# put a 32+ character value in Jwt:Key, or:
dotnet user-secrets --project WebService/SmartSolar.Api set "Jwt:Key" "<your-32-char-key>"
```

Start (from the repo root):

```bash
dotnet run --project "WebService/SmartSolar.Api/SmartSolar.Api.csproj" --launch-profile lan
```

Done when:

1. On the Mac: `curl http://<mac-ip>:5080/health` returns `{ "status": "Healthy", ... }`.
2. On your phone (same Wi-Fi): open `http://<mac-ip>:5080/health`.
3. Swagger on the phone or another laptop: `http://<mac-ip>:5080/swagger` → `POST /api/auth/login` with `admin` / `Admin@123` returns a token.

If the IP changes after a router reboot, run `ipconfig getifaddr en0` and update the group. Prefer a DHCP reservation on the router for the Mac.

To pin a static address on macOS: **System Settings → Wi-Fi → Details → TCP/IP → Configure IPv4: Manually**. Keep the same subnet and router the DHCP lease used.

### MVC web app (local run)

The web client is `WebApp/SmartSolar.Web`. It stores the JWT in session and calls `http://localhost:5080/api/`. No MongoDB connection from the web project.

```bash
dotnet run --project "WebApp/SmartSolar.Web/SmartSolar.Web.csproj"
```

Open `http://localhost:5081`. Seed logins: `admin / Admin@123` (Backoffice dashboard), `operator1 / Oper@123` (Operations home).

---

## Windows IIS host (marks / submission)

Do this on the Windows PC that will stay on for the demo. Install **MongoDB Community + Compass** on that same PC so the app pool can use `mongodb://localhost:27017`.

### 1. Enable IIS

1. **Control Panel → Programs → Turn Windows features on or off**.
2. Tick **Internet Information Services**.
3. Under **World Wide Web Services → Application Development Features**, tick **WebSocket Protocol** if listed (optional).
4. OK, wait for install.

Or from an elevated PowerShell:

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-ManagementConsole -All
```

### 2. Install the .NET Hosting Bundle

The API targets **.NET 10**. Download the **ASP.NET Core 10 Runtime - Windows Hosting Bundle** from Microsoft’s .NET 10 download page and install it.

Then **restart IIS** so `AspNetCoreModuleV2` loads:

```powershell
net stop was /y
net start w3svc
```

If you skip this restart you get **500.19** or a blank 500 because the module is missing.

### 3. Publish (framework-dependent, win-x64)

On the Windows PC, from the repo:

```powershell
cd "WebService\SmartSolar.Api"
dotnet publish -c Release --self-contained false -o "C:\inetpub\smart-solar-api"
```

From this Mac you can produce the same folder (copy it across with USB / zip):

```bash
dotnet publish "WebService/SmartSolar.Api/SmartSolar.Api.csproj" \
  -c Release --self-contained false \
  -o "./publish/SmartSolar.Api"
```

`publish/` is gitignored. The Hosting Bundle on Windows supplies the runtime, so the publish is **not** self-contained.

Create the log folder IIS will write to:

```powershell
mkdir "C:\inetpub\smart-solar-api\logs"
```

### 4. Secrets on the machine (never in Git)

Copy the example file and edit it **only on the server**:

```powershell
copy C:\inetpub\smart-solar-api\appsettings.Production.json.example `
     C:\inetpub\smart-solar-api\appsettings.Production.json
```

Set:

- `MongoDb:ConnectionString` — `mongodb://localhost:27017` if Mongo is on the same PC.
- `Jwt:Key` — a new random string, **at least 32 characters**. Do not reuse the Development key.

Alternatively set IIS environment variables (double underscore = nested config):

| Variable | Example |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `MongoDb__ConnectionString` | `mongodb://localhost:27017` |
| `MongoDb__DatabaseName` | `SmartSolar` |
| `Jwt__Key` | *(long random secret)* |
| `Jwt__Issuer` | `SmartSolar.Api` |
| `Jwt__Audience` | `SmartSolar.Clients` |

`web.config` in the project already sets `ASPNETCORE_ENVIRONMENT=Production` and turns on stdout logging. It does **not** contain the JWT key.

### 5. App pool: No Managed Code

1. **IIS Manager → Application Pools → Add Application Pool**.
2. Name: `SmartSolarApi`.
3. **.NET CLR version: No Managed Code**.
4. Pipeline: Integrated.
5. Advanced Settings → **Identity**: `ApplicationPoolIdentity` is fine.

ANCM (not the old CLR) loads .NET. That is why the pool is **No Managed Code**. Viva: IIS is a reverse proxy; Kestrel runs **out of process**.

### 6. Site + permissions

1. **Sites → Add Website**.
2. Site name: `SmartSolarApi`.
3. App pool: `SmartSolarApi`.
4. Physical path: `C:\inetpub\smart-solar-api`.
5. Binding: **http**, IP **All Unassigned**, port **5080**, host name blank.
6. Grant the pool identity read/execute on the folder:

```powershell
icacls "C:\inetpub\smart-solar-api" /grant "IIS AppPool\SmartSolarApi:(OI)(CI)RX" /T
icacls "C:\inetpub\smart-solar-api\logs" /grant "IIS AppPool\SmartSolarApi:(OI)(CI)M" /T
```

### 7. Static LAN IP

On the Windows PC:

1. **Settings → Network & internet → Ethernet/Wi-Fi → Edit IP assignment → Manual**.
2. IPv4 on, pick an address on the same subnet as the phones (example `192.168.1.50`), subnet `255.255.255.0`, gateway = your router.
3. Better: a **DHCP reservation** on the router for this PC’s MAC, so it keeps the same IP.

Record the IPv4. That is the host in the group announcement.

### 8. Windows Firewall inbound rule

Elevated PowerShell:

```powershell
New-NetFirewallRule -DisplayName "Smart Solar API 5080" `
  -Direction Inbound -Protocol TCP -LocalPort 5080 -Action Allow
```

Without this, `/health` works on the PC and times out on a phone.

### 9. Smoke test

From the Windows PC:

```powershell
curl http://localhost:5080/health
```

From a phone on the same Wi-Fi:

```
http://<windows-ip>:5080/health
http://<windows-ip>:5080/swagger
```

Login:

```powershell
curl -Method POST http://<windows-ip>:5080/api/auth/login `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"Admin@123"}'
```

Done when the phone gets `Healthy` and login returns `token`.

---

### 10. Host the web app (second IIS site)

The steps above host only the API. Do the same for `WebApp/SmartSolar.Web`, with these differences:

1. Publish: `dotnet publish -c Release --self-contained false -o "C:\inetpub\smart-solar-web"` from `WebApp\SmartSolar.Web`.
2. New app pool `SmartSolarWeb`, **No Managed Code**. New site `SmartSolarWeb`, physical path `C:\inetpub\smart-solar-web`, http binding on port **5081**. Grant the pool identity read/execute on the folder (same `icacls` pattern as step 6).
3. Firewall rule for port 5081, same as step 8.
4. Point the web app at the API. It reads `Api:BaseUrl` (default `http://localhost:5080/api/` in `appsettings.json`, with the trailing slash). If both sites are on the same PC, the default works. If the API is elsewhere, create `appsettings.Production.json` in the web site folder on the server with `{ "Api": { "BaseUrl": "http://<api-ip>:5080/api/" } }`, or set the environment variable `Api__BaseUrl`.
5. The web app has no database. It only calls the API, so start the API site first.
6. Login sessions are kept in memory. If the app pool recycles or restarts, users are signed out and must log in again. That is expected.
7. Test: open `http://<windows-ip>:5081` from a phone or another PC, log in as `admin / Admin@123`, and open the Users page. Log in as `operator1` and confirm the Users page is denied.

## JWT key check at startup

The example production file ships with a placeholder `Jwt:Key` value (`REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS`). The API **refuses to start** if the key is blank, shorter than 32 characters, or still exactly that placeholder, so a forgotten key cannot let anyone forge tokens.

On IIS a refused start shows as **502.5**. Open `logs\stdout*.log` in the site folder and look for "Jwt:Key is still the example placeholder". Fix it by setting a real random key (at least 32 characters) in `appsettings.Production.json` or the `Jwt__Key` environment variable, then recycle the app pool.

Generate a key on Windows PowerShell with: `[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))`

## Troubleshooting (know these for the viva)

| Symptom | Likely cause | Fix |
|---|---|---|
| **500.19** | Hosting Bundle / `AspNetCoreModuleV2` missing, or `web.config` cannot be read | Install Hosting Bundle, `net stop was /y` then `net start w3svc`, check folder ACLs |
| **502.5** | App process crashed on start | Read `C:\inetpub\smart-solar-api\logs\stdout_*.log`. Usual causes: missing `Jwt:Key`, Mongo not running, wrong connection string |
| **404** on every path | Site points at the wrong folder (not the publish output that contains `SmartSolar.Api.dll` and `web.config`) | Fix physical path |
| Phone times out, localhost works | Firewall, or site bound to `127.0.0.1` only | Binding = All Unassigned; add the inbound rule |
| `Jwt:Key must be at least 32 characters` in stdout | Production config not applied | Set `Jwt__Key` or `appsettings.Production.json` |
| `Jwt:Key is still the example placeholder` in stdout | Copied the `.example` file and left the sample key | Generate a random 32+ character key; do not use the placeholder |
| Mongo timeout | Mongo service stopped, or app pool cannot reach `localhost` | Start MongoDB; Compass should still connect |
| HTTPS redirect / connection reset | Site is HTTP only | This API does **not** redirect to HTTPS on purpose (LAN lab) |

---

## Milestone 0 checklist

- [x] Every `.cs` file has the header block; every method has an inline comment
- [x] JWT signing key is not in Git. `appsettings.Development.json` and `appsettings.Production.json` are gitignored; committed copies are `.example` placeholders. Local: user-secrets or a private Development file. IIS: `Jwt__Key` env var.
- [x] `GET /api/users` without a token → 401; Prosumer token → 403; Backoffice token → 200
- [x] Kestrel listens on `0.0.0.0:5080` so a second device on the Wi-Fi can call `/health` and `/api/auth/login`
- [ ] IIS site on Windows (Hosting Bundle, No Managed Code, static IP, firewall) — run the Windows section on the lab PC, then tick this

`publish/`, `appsettings.Development.json` and `appsettings.Production.json` are in `.gitignore`.
