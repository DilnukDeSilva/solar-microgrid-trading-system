# Member 1: Service Foundation, Database, Auth, IIS/LAN, Web Login & Users

**You start first. Every other member is blocked on your Milestone 0.** Finish it in about 2 days and tell the group chat.

## Marks you own
- Group: Service Architecture & API Design (8), Database Design (4), Documentation & Deployment (part)
- Individual: Web login + role-based access (4), Web user management (4), plus Service Integration (Web ↔ API) (2)

## Milestone 0: unblocks everyone (Day 1–2)
1. GitHub repo with folders `WebService/`, `WebApp/`, `MobileApp/`, `docs/`. Add `main` protected, `dev` branch, one branch per feature, and a PR template. Invite all members.
2. Commit `PLAN.md`, `API-CONTRACT.md` and the member files.
3. ASP.NET Core Web API project that builds. Add Swagger (all members test through it), CORS, and the global error format from the contract.
4. MongoDB connection via configuration (no hard-coded strings) and the **four collections** with model classes matching the contract. Create indexes: unique NIC, unique username.
5. Seed script or seeder that loads the seed data listed in the contract.
6. Auth: `POST /auth/login`, JWT issuing and validation, password hashing, role-based authorization policies the others can reuse (`[Authorize(Roles=...)]`).
7. `GET /health` reachable from another device on the LAN, after **publishing to IIS** (install .NET Hosting Bundle, app pool "No Managed Code", firewall inbound rule, static IP). Write the exact steps in `docs/DEPLOYMENT.md` as you go. That is your reproducible deployment mark.
8. Announce the base URL and test credentials to the group.

## Milestone 1: your features (Day 3–6)
- `Users` staff API (`/users`): create Backoffice/GridOperator accounts, list, update, deactivate. Backoffice only.
- Web app skeleton (ASP.NET Core MVC + Bootstrap 5): shared layout, navbar that changes by role, login page, session/cookie storing the JWT, an API client class all web pages reuse (M2 depends on this), 403 page, logout.
- Role-based redirect after login: Backoffice → admin dashboard, GridOperator → operations home.
- User management pages: list, create, edit, deactivate.

Hand the web skeleton + API client to M2 by **end of Day 4**.

## Milestone 2 (Day 7+)
- Help integrate everything on the IIS box, run a full LAN test with a real phone, and support the smoke test checklist.
- Lead the report's architecture, DB design and deployment sections.

## Depends on / blocks
- Depends on: nothing.
- Blocks: M2, M3, M4 (all need login, DB and the reachable server). Also M2 needs your web skeleton.

## Definition of done
- Every `.cs` file has the comment header block, and every method starts with an inline comment.
- No business rules in the web controllers or views.
- Swagger shows all your endpoints with the correct role protection.
- A wrong-role call returns 403 with the contract error body.

## Viva prep: be able to explain
How JWT works and where the secret lives, how the app pool and hosting bundle run your API, why NIC and username indexes are unique, what FAT service means, why clients never touch MongoDB, how passwords are stored.
