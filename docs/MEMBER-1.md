# Member 1: Service Foundation, Database, Auth, Web Login & Users

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
7. `GET /health` works locally and on the LAN from your Mac (Kestrel bound to all interfaces). **IIS publishing needs Windows, so it moved to Member 2** (see `MEMBER-2.md`, "IIS deployment"). You supply the working code, config example files and `docs/DEPLOYMENT.md`, and support M2 remotely.
8. Announce the base URL and test credentials to the group.

## Milestone 1: your features (Day 3–6)
- `Users` staff API (`/users`): create Backoffice/GridOperator accounts, list, update, deactivate. Backoffice only.
- Web app skeleton (ASP.NET Core MVC + Bootstrap 5): shared layout, navbar that changes by role, login page, session/cookie storing the JWT, an API client class all web pages reuse (M2 depends on this), 403 page, logout.
- Role-based redirect after login: Backoffice → admin dashboard, GridOperator → operations home.
- User management pages: list, create, edit, deactivate.

Hand the web skeleton + API client to M2 by **end of Day 4**.

## Milestone 2 (Day 7+)
- Support M2 during the IIS deployment (remote help, config questions, fixing API-side startup errors). M2 runs the IIS steps, since it needs a Windows PC.
- Lead the report's architecture and DB design sections. M2 leads the deployment section, and you review it.

## Depends on / blocks
- Depends on: nothing.
- Blocks: M2, M3, M4 (all need login, DB and the reachable server). Also M2 needs your web skeleton.

## Definition of done
- Every `.cs` file has the comment header block, and every method starts with an inline comment.
- No business rules in the web controllers or views.
- Swagger shows all your endpoints with the correct role protection.
- A wrong-role call returns 403 with the contract error body.

## Viva prep: be able to explain
How JWT works and where the secret lives, how configuration reaches the API in production (environment variables, not source), why NIC and username indexes are unique, what FAT service means, why clients never touch MongoDB, how passwords are stored.

## Status
- [x] Milestone 0 API (IIS hosting handed to Member 2, Windows only)
- [x] Staff Users API: create, list, update, deactivate (Backoffice only)
- [x] MVC + Bootstrap 5 skeleton: layout, login, session JWT, `ApiClient`, 403, logout
- [x] Role redirects: Backoffice → admin dashboard, GridOperator → operations home
- [x] User management pages: list, create, edit, deactivate

Still open:
- [x] Commit and push Users API + web app on `feature/m1-service-auth-iis`
- [ ] Open the pull request into `dev`, then tell the group to pull (M2 needs this first for IIS)
- [x] Update `API-CONTRACT.md` with the staff rules (`USERNAME_EXISTS`, password minimum 8)
- [x] Final pass: header block and method comments on every `.cs` file (66 files, 109 methods checked, none missing)
- [ ] Support M2 on IIS setup (remote), then review the deployment doc

Moved to Member 2 (needs a Windows PC): IIS deployment of the API and web app, LAN test with a real phone, smoke-test checklist.
