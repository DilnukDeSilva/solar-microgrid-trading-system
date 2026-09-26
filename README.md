# Smart Solar Microgrid Trading System (SE4040 EAD Assignment 1)

Client-server system: C# Web API on IIS + MongoDB, an ASP.NET Core web app, and a native Android app with SQLite.

| Folder | Content | Owner |
|---|---|---|
| `WebService/` | C# Web API + MongoDB | all (each member owns their own controllers) |
| `WebApp/` | Web application (Bootstrap 5) | M1 (skeleton, users), M2 (operations), M3 (pending activation) |
| `MobileApp/` | Native Android + SQLite | M3 (foundation, account, booking), M4 (dashboard, maps, QR, operator) |
| `docs/` | Plan, API contract, member task files | all |

Start with `docs/PLAN.md`, `docs/API-CONTRACT.md` and your `docs/MEMBER-<n>.md`.

## Git workflow
- `main`: protected, submission-ready only. `dev`: integration branch.
- Each member works on their own branch and merges to `dev` by pull request.
- Small, descriptive commits (the report needs per-member contribution evidence).

| Branch | Member |
|---|---|
| `feature/-service-auth-iis` | Member 1 |
| `feature/-web-operations` | Member 2 |
| `feature/-mobile-account-booking` | Member 3 |
| `feature/-mobile-maps-qr-operator` | Member 4 |

## TODO before submission
- [ ] Individual contributions per member (with links to commits)
- [ ] Video link (max 5 min)
- [ ] Report, diagrams, screenshots
