# M1 Milestone 0 Runbook (planning guide, not code)

Goal: by end of Day 2 the other three members can log in against a server they reach over the LAN. Do the steps in this order. Each has a "done when" check. Write the code yourself and cite any snippet you adapt.

## Step 1: Repo (30 min)
- Create the GitHub repo, add the three others as collaborators, protect `main`, work on `dev` and feature branches.
- Add a `.gitignore` for Visual Studio/.NET, Android and Node. **Never commit** connection strings, the JWT secret or Google Maps keys.
- Done when: all four can clone, and the plan and contract files are on `dev`.

## Step 2: Web API project
- Create an ASP.NET Core Web API project (C#) in `WebService/`. Use controllers, not minimal APIs, so every member's endpoints live in their own controller file.
- Layer it: Controllers (thin) → Services (business rules) → Repositories (Mongo access) → Models/DTOs. This is what "FAT service" means in your viva.
- Enable Swagger and make it accept a bearer token so testers can authorize.
- Add a global exception handler that returns the `{code, message}` body from the contract.
- Add the comment header and method comments from day one.
- Done when: Swagger opens locally and an unhandled error returns the contract error shape.

## Step 3: MongoDB
- Install MongoDB Community + Compass and make sure the service runs.
- Read the connection string and database name from `appsettings` (and an environment override for IIS), never from code.
- Add the official **MongoDB.Driver** NuGet package.
- Create model classes for the four collections exactly as in `API-CONTRACT.md`. Store enums as strings and dates as UTC.
- Create indexes: unique on user NIC and on username; index on reservation `prosumerNic`, `scheduledAt` and `status`.
- Done when: the API starts, connects, and Compass shows the four collections.

## Step 4: Seed data
- Build a seeder that runs at startup only when Users is empty (idempotent), or a separate mongo script. Use the exact seed list in the contract.
- Passwords are stored as hashes. Research ASP.NET Core `PasswordHasher` or BCrypt.
- Done when: re-running does not duplicate data, and Compass shows all sample users, 3 stations, slots and reservations in each status.

## Step 4b: Share the contract-first mock
- Until Step 6 is done, export a couple of sample JSON responses into `docs/mock/` so M2–M4 can start.

## Step 5: Auth
- Concepts to read first: JWT structure (header, payload, signature), claims, expiry, HMAC signing key, `Authorization: Bearer`.
- Implement `POST /auth/login` (username or NIC + password) → token, expiry, user. Reject `Deactivated`/`Pending` accounts with the right code.
- Add JWT bearer authentication and role-based authorization to the pipeline. Claims per the contract: `sub`, `role`, `nic`.
- Keep the signing key in configuration or user-secrets, never in the repo.
- Done when: Swagger call without a token → 401, with a Prosumer token on a Backoffice endpoint → 403, with the right role → 200.

## Step 6: `/health` and CORS
- A simple anonymous `GET /health` returning status and server time.
- Enable CORS as needed for the web app. Mobile does not need CORS.
- Done when: it responds locally.

## Step 7: IIS + LAN hosting
- On the Windows host: enable IIS, install the **.NET Hosting Bundle**, restart IIS.
- Publish the API (framework-dependent), create a site pointing at the publish folder, app pool set to **No Managed Code**.
- Give the app pool identity read access to the publish folder. Set environment/config values for the Mongo connection and JWT key.
- Bind the site to all IPs on a chosen port, set a **static LAN IP** (or DHCP reservation), and add a **Windows Firewall inbound rule** for that port.
- Common failures to know: 500.19 (missing hosting bundle/module), 502.5 (app crashed on startup, check `stdout` logs), Mongo unreachable from the app pool, firewall blocking the port.
- Done when: a phone on the same Wi-Fi opens `http://<pc-ip>:<port>/health` and gets a response, then `POST /auth/login` works from Swagger through IIS.

## Step 8: Announce and document
- Write `docs/DEPLOYMENT.md` with the exact steps you just did, so anyone can reproduce them. This earns the reproducible-deployment mark.
- Message the group: base URL, seed credentials, the Swagger URL.
- Done when: M2, M3 and M4 each confirm they can log in.

## Search terms for the official docs
"ASP.NET Core Web API controllers", "MongoDB C# driver quick start", "ASP.NET Core JWT bearer authentication", "role-based authorization ASP.NET Core", "host ASP.NET Core on Windows with IIS", "ASP.NET Core Module troubleshooting".

## Before you tell the group you're done
- [x] Every `.cs` file has the header block and inline method comments
- [x] No secrets in Git history (`appsettings.Development.json` and `appsettings.Production.json` gitignored; IIS uses env vars)
- [x] 401/403/200 verified for each role
- [x] Reachable from a second device on the LAN (`0.0.0.0:5080` — see `docs/DEPLOYMENT.md`)
- [ ] IIS on the Windows lab PC (Hosting Bundle, No Managed Code, firewall) — follow `docs/DEPLOYMENT.md`
