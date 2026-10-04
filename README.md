# Smart Solar Microgrid Trading System (SE4040 EAD Assignment 1)

Client-server system: C# Web API on IIS + MongoDB, an ASP.NET Core web app, and a native Android app with SQLite.

| Folder | Content | Owner |
|---|---|---|
| `WebService/` | C# Web API + MongoDB | all (each member owns their own controllers) |
| `WebApp/` | Web application (Bootstrap 5) | M1 (skeleton, users), M2 (operations), M3 (pending activation) |
| `MobileApp/` | Native Android + SQLite | M3 (foundation, account, booking), M4 (dashboard, maps, QR, operator) |

## Run locally (Member 1)

JWT key is not in Git. Copy `WebService/SmartSolar.Api/appsettings.Development.json.example` to `appsettings.Development.json` (or `dotnet user-secrets`) then:

```bash
dotnet run --project "WebService/SmartSolar.Api/SmartSolar.Api.csproj" --launch-profile lan
dotnet run --project "WebApp/SmartSolar.Web/SmartSolar.Web.csproj"
```

API: `http://localhost:5080/swagger` · Web: `http://localhost:5081` · Health: `http://localhost:5080/health`

Seed: `admin / Admin@123` (Backoffice), `operator1 / Oper@123` (GridOperator).

## Git workflow
- `main`: protected, submission-ready only. `dev`: integration branch.
- Each member works on their own branch and merges to `dev` by pull request.
- Small, descriptive commits (the report needs per-member contribution evidence).

| Branch | Member |
|---|---|
| `feature/service-auth-iis` | Member 1 |
| `feature/web-operations` | Member 2 |
| `feature/mobile-account-booking` | Member 3 |
| `feature/mobile-maps-qr-operator` | Member 4 |

## TODO before submission
- [ ] Individual contributions per member (with links to commits)
- [ ] Video link (max 5 min)
- [ ] Report, diagrams, screenshots
