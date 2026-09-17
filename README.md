# RaceDay — ASP.NET Core Edition

RaceDay is a race/event management platform. **Organisers** create events
with categories, review who has entered, and record results. **Participants**
browse events, enrol, and track their own entries, results and personal
bests.

This is a migration of an existing React + TypeScript + Supabase prototype
(kept in `legacy-react-app/` for reference) into a production-shaped
.NET solution:

```
ASP.NET Core MVC  →  ASP.NET Core Web API  →  Entity Framework Core  →  SQL Server
                                            ↘  Azure Blob Storage (event banners, avatars)
```

See `docs/MIGRATION_MAP.md` for the full feature-by-feature mapping from
the original app, and the **Verification Status** section below for an
honest, itemised account of what has and hasn't been run in this
environment.

## Architecture

| Project | Responsibility |
|---|---|
| `RaceDay.Domain` | POCO entities and enums. No dependencies. |
| `RaceDay.Infrastructure` | EF Core `DbContext` + Fluent configuration, Azure Blob / local-disk storage abstraction. |
| `RaceDay.Api` | ASP.NET Core Web API. Owns all data access via EF Core. Issues/validates JWTs. Only project that touches SQL Server. |
| `RaceDay.Web` | ASP.NET Core MVC. Talks to `RaceDay.Api` over HTTP only — no EF Core / SQL Server reference at all. Cookie-authenticates the browser and forwards the API's JWT on every API call. |
| `RaceDay.Api.Tests` | xUnit + `WebApplicationFactory<Program>` integration tests, running against an in-memory SQLite database. |

RaceDay.Web never talks to SQL Server or EF Core directly — every read and
write goes through `IRaceDayApiClient` → HTTP → `RaceDay.Api`.

## Roles

- **Participant** — browse/search events, enrol in a category, view own
  enrolments and results, edit profile, upload an avatar.
- **Organiser** — full CRUD on their own events and categories, view
  enrolments for their events, record/edit results. Cannot manage another
  organiser's events (enforced server-side — see `docs/RaceDay_API_Endpoint_Plan.md`).

## Technology stack

C# · ASP.NET Core 8 · ASP.NET Core MVC · ASP.NET Core Web API ·
Entity Framework Core 8 · SQL Server · JWT Bearer auth · Azure.Storage.Blobs ·
Swagger/Swashbuckle · xUnit · Docker · GitHub Actions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, a full instance, or the `docker-compose.yml` SQL Server container)
- (Optional) an Azure Storage account, for real blob storage — otherwise the app
  automatically falls back to writing under `RaceDay.Api/wwwroot/uploads`.

## Configuration

Nothing secret is committed. Set these via `dotnet user-secrets`,
environment variables, or your deployment platform's secret store:

**`RaceDay.Api`**
```
ConnectionStrings:RaceDayDb   SQL Server connection string
Jwt:Key                       ≥32-character signing secret
Jwt:Issuer / Jwt:Audience     defaults provided in appsettings.json
Storage:Provider              "Azure" or "Local" (defaults to Local)
Storage:AzureConnectionString required only when Storage:Provider = Azure
Storage:ContainerName         defaults to "raceday-media"
Cors:AllowedOrigins           must include RaceDay.Web's origin
```

**`RaceDay.Web`**
```
Api:BaseUrl                   e.g. https://localhost:7051/
```

Example (development, from the repo root):
```bash
cd src/RaceDay.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "a-really-long-random-development-only-secret"
```

## Running locally

```bash
# 1. Restore & build
dotnet restore RaceDay.sln
dotnet build RaceDay.sln

# 2. Create/update the database (from the repo root)
dotnet ef database update --project src/RaceDay.Infrastructure --startup-project src/RaceDay.Api

# 3. Run the API (terminal 1)
dotnet run --project src/RaceDay.Api

# 4. Run the MVC site (terminal 2), pointing Api:BaseUrl at the API above
dotnet run --project src/RaceDay.Web
```

Swagger UI: `https://localhost:<api-port>/swagger`
MVC site: `https://localhost:<web-port>/`

### Creating the first EF Core migration

No migration has been generated in this environment (no .NET SDK is
available here — see Verification Status). Generate it yourself:

```bash
dotnet tool install --global dotnet-ef   # if not already installed
cd src/RaceDay.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../RaceDay.Api
dotnet ef database update --startup-project ../RaceDay.Api
```

Alternatively, run `docs/RaceDay_Database.sql` directly against a clean
SQL Server instance — it defines the identical schema by hand, including
seed data for 2 organisers, 2 participants, 3 events, categories per
event, and sample enrolments/results.

### Seeding realistic login credentials

The SQL script's seeded profiles use a placeholder password hash (SQL
alone can't call `PasswordHasher<Profile>`). To get real, loggable-in seed
accounts, either:
- register them through `POST /api/auth/register` / the MVC **Sign up** page, or
- add an EF Core `Database.EnsureCreated`/seed method in `Program.cs` (dev-only) that calls `IPasswordHasher<Profile>` in-process before saving.

## Running tests

```bash
dotnet test RaceDay.sln
```

`RaceDay.Api.Tests` boots the real API pipeline (`WebApplicationFactory<Program>`)
against an in-memory SQLite database, and covers:
- registration, duplicate email, login, invalid credentials
- anonymous → protected endpoint → 401
- participant → organiser-only endpoint → 403 (event creation, result recording)
- organiser → another organiser's event → 403
- organiser → own event → succeeds
- event CRUD, category creation, enrolment creation + duplicate prevention
- result creation/update, participant read-only access to their own results

## Docker

```bash
docker build --target api -t raceday-api .
docker build --target web -t raceday-web .

# or bring up API + Web + SQL Server together:
docker compose up --build
```

## CI — GitHub Actions

`.github/workflows/ci.yml` runs on every push/PR: validates the repo
structure, then `dotnet restore` → `build` → `test` (Release configuration),
then builds both Docker images. The workflow fails the run if the build or
any test fails — there is no way for it to report a false green build.

## Project structure

```
RaceDay.sln
src/
  RaceDay.Domain/          entities, enums
  RaceDay.Infrastructure/  DbContext, EF configurations, blob storage
  RaceDay.Api/             controllers, DTOs, services, JWT auth, Program.cs
  RaceDay.Web/              MVC controllers, Razor views, typed API client
tests/
  RaceDay.Api.Tests/       xUnit integration tests
docs/
  RaceDay_ERD.png
  RaceDay_API_Endpoint_Plan.md
  RaceDay_Database.sql
  MIGRATION_MAP.md
legacy-react-app/          original React/Supabase prototype, kept for reference only
Dockerfile
docker-compose.yml
.github/workflows/ci.yml
```

---

## Verification status

Per the project brief's own rule: **nothing below is marked done unless it
was actually run in this environment.** This sandbox has no .NET SDK, no
SQL Server, no Azure subscription, no Docker daemon, and restricted
outbound network access (NuGet was unreachable), so the code was written
by hand and reviewed, but **not compiled, executed, migrated, containerised,
or tested here.**

| Requirement | Status | Note |
|---|---|---|
| Audit of existing React/Supabase project | IMPLEMENTED | See `docs/MIGRATION_MAP.md`, based on reading every source/page/service/migration file. |
| Solution structure (`RaceDay.sln`, 4 projects + tests) | IMPLEMENTED | Real `.sln` + `.csproj` files with correct project references. |
| Domain entities | IMPLEMENTED | Matches the audited schema; documented deviation (`Role` on `Profile`, not a separate table) explained in the migration map. |
| EF Core `DbContext` + Fluent configuration | IMPLEMENTED (code) / CANNOT VERIFY (compiles/runs) | No `dotnet build` available here. |
| EF Core migrations | NOT IMPLEMENTED | No `dotnet-ef` tool / SDK available in this sandbox to generate them. Run the `dotnet ef migrations add` command in the README yourself. |
| SQL Server schema script | IMPLEMENTED (written) / CANNOT VERIFY (executed) | `docs/RaceDay_Database.sql` — no SQL Server instance available here to run it against. |
| Seed data (2 organisers, 2 participants, 3 events, categories, enrolments, a result) | IMPLEMENTED (script) | Password hashes are placeholders — see "Seeding" above. |
| JWT authentication (register/login/logout) | IMPLEMENTED (code) | `AuthService`, `TokenService`; not run end-to-end here. |
| Role-based authorization, server-side ownership checks | IMPLEMENTED (code) | Enforced in each `*Service`, never trusts client-supplied IDs; covered by `AuthorizationTests.cs`. |
| Web API — all endpoints in the brief | IMPLEMENTED (code) | See `docs/RaceDay_API_Endpoint_Plan.md` for the full route table and status codes. |
| ASP.NET Core MVC client, consuming the API only | IMPLEMENTED (code) | `RaceDay.Web` has no EF Core/SQL Server reference; all data access via `IRaceDayApiClient`. |
| Participant workflows (dashboard, browse/filter, enrol, my events/results, profile + avatar) | IMPLEMENTED (code) | Full controllers + Razor views. |
| Organiser workflows (dashboard, event/category CRUD, enrolments, results) | IMPLEMENTED (code) | Full controllers + Razor views. |
| Azure Blob Storage integration, with local-dev fallback | IMPLEMENTED (code) / CANNOT VERIFY (uploaded) | `AzureBlobStorageService` + `LocalFileStorageService`; no Azure subscription available here to exercise the real path. |
| Validation (client, server, DB) | IMPLEMENTED | DataAnnotations on DTOs/ViewModels + unique indexes/CHECK constraints in EF config and SQL. |
| Error handling (no leaked internals) | IMPLEMENTED | `ExceptionHandlingMiddleware` maps domain exceptions to 400/401/403/404/409, logs 500s server-side only. |
| Automated tests | IMPLEMENTED (code) / CANNOT VERIFY (executed) | `RaceDay.Api.Tests` — 12 xUnit tests across auth, authorization, events, enrolments, results. Never actually run — no SDK here. **Run `dotnet test` yourself and report the real result.** |
| Dockerfile | IMPLEMENTED (written) / CANNOT VERIFY (built) | Multi-stage build for both API and Web images; not built here (no Docker daemon). |
| GitHub Actions CI | IMPLEMENTED (written) / CANNOT VERIFY (green) | `.github/workflows/ci.yml`; will only go green once it actually runs on GitHub against a real commit. |
| README / docs / ERD / endpoint plan / SQL script | IMPLEMENTED | This file, `docs/RaceDay_ERD.png` (rendered), `docs/RaceDay_API_Endpoint_Plan.md`, `docs/RaceDay_Database.sql`. |
| Supabase fully removed from the production path | IMPLEMENTED (new code) / NOT DONE (old code not deleted) | The new .NET stack has zero Supabase dependency. The original `project/` folder (renamed `legacy-react-app/`) was **not deleted**, per the brief's own instruction not to remove working functionality until its replacement exists and is verified — and it hasn't been verified here yet. Delete it once you've run the checks above yourself. |

### What you need to run, in order, to actually verify this

1. `dotnet restore RaceDay.sln && dotnet build RaceDay.sln` — confirms everything compiles. It has not been compiled in this sandbox; expect to fix minor issues (a missing `using`, a package version mismatch) on the first try, as is normal for hand-written code that's never seen a compiler.
2. `dotnet ef migrations add InitialCreate ...` then `dotnet ef database update ...` against a real SQL Server/LocalDB — or run `docs/RaceDay_Database.sql` directly.
3. `dotnet test RaceDay.sln` — confirms the 12 integration tests actually pass.
4. Run both apps locally, register an Organiser and a Participant, and walk the two workflows in Step 19 of the brief by hand.
5. `docker compose up --build` to confirm the containers actually start.
6. Push to GitHub and confirm the Actions workflow goes green.
