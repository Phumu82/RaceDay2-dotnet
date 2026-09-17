# RaceDay

RaceDay is a .NET 10 race management platform with two apps:
1. **RaceDay.Api**: REST API for auth, events, categories, enrolments, results, and profile media.
2. **RaceDay.Web**: ASP.NET Core MVC front end (Razor views) that consumes the API over HTTP.

## What it does

- **Participants** can browse/filter events, enrol, and view their own events and results.
- **Organisers** can create/manage their events and categories, view enrolments, and record results.
- Profile avatars and event banners are stored via an abstraction that supports **Azure Blob Storage** or **local file storage**.

## Solution architecture

| Project | Purpose |
|---|---|
| `src/RaceDay.Domain` | Domain entities and enums |
| `src/RaceDay.Infrastructure` | EF Core data access, SQL Server provider, storage implementations |
| `src/RaceDay.Api` | Web API, JWT auth, validation, authorization, business services |
| `src/RaceDay.Web` | MVC UI, cookie auth, typed API client |
| `tests/RaceDay.Api.Tests` | Integration tests for API/auth/authorization flows |

**Important:** `RaceDay.Web` does not access SQL Server directly. It only calls `RaceDay.Api`.

## Tech stack

- .NET 10
- ASP.NET Core MVC + Web API
- Entity Framework Core 10 (SQL Server)
- JWT bearer auth (API) + cookie auth (Web)
- Swagger (API, Development environment)
- xUnit + `WebApplicationFactory` integration testing
- Docker / Docker Compose

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)
- Optional: Azure Storage account (if using Blob storage in non-local mode)
- Optional: Docker Desktop (for containerized run)

## Configuration

Use `appsettings.*.json`, environment variables, and/or user-secrets.

### RaceDay.Api settings

| Key | Required | Notes |
|---|---|---|
| `ConnectionStrings:RaceDayDb` | Yes (for SQL Server mode) | SQL Server connection string |
| `Jwt:Key` | Yes | Use a strong 32+ char secret |
| `Jwt:Issuer` | No | Default provided |
| `Jwt:Audience` | No | Default provided |
| `UseSqlServer` | No | Defaults to `true`; tests can disable |
| `Storage:Provider` | No | `Local` or `Azure` |
| `Storage:AzureConnectionString` | Required for Azure | Enables Azure blob implementation |
| `Storage:ContainerName` | No | Defaults to `raceday-media` |
| `Cors:AllowedOrigins` | Yes for browser calls | Must include Web app origin |

### RaceDay.Web settings

| Key | Required | Notes |
|---|---|---|
| `Api:BaseUrl` | Yes | Base URL for API |
| `API_BASE_URL` (env var) | Optional override | Preferred in cloud environments |

`RaceDay.Web` validates API URL at startup:
- URL must be absolute.
- In non-Development environments, loopback/localhost API URLs are rejected.

## Local development quick start

From repository root:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
```

Set API secret (development example):

```bash
cd src/RaceDay.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "DEV-ONLY-CHANGE-TO-A-LONG-RANDOM-SECRET-32-CHARS+"
cd ../..
```

Apply database schema (if using EF migrations):

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/RaceDay.Infrastructure --startup-project src/RaceDay.Api
```

Run both apps in separate terminals:

```bash
dotnet run --project src/RaceDay.Api
dotnet run --project src/RaceDay.Web
```

Default development URLs:
- API: `https://localhost:7051` (`http://localhost:5051`)
- Web: `https://localhost:7050` (`http://localhost:5050`)
- Swagger: `https://localhost:7051/swagger`

## Database options

1. **EF Core migrations workflow** (recommended for evolving schema).
2. **SQL script workflow** using `docs/RaceDay_Database.sql`.

If you do not yet have migrations in this repository, generate initial migration:

```bash
dotnet ef migrations add InitialCreate --project src/RaceDay.Infrastructure --startup-project src/RaceDay.Api
dotnet ef database update --project src/RaceDay.Infrastructure --startup-project src/RaceDay.Api
```

## Running tests

```bash
dotnet test RaceDay.sln
```

Tests are in `tests/RaceDay.Api.Tests` and cover auth, authorization boundaries, and core event flows.

## Docker

Build images:

```bash
docker build --target api -t raceday-api .
docker build --target web -t raceday-web .
```

Run full stack:

```bash
docker compose up --build
```

Compose includes:
- SQL Server
- API container
- Web container

## Deploying to cloud (minimum checklist)

1. Deploy `RaceDay.Api` and `RaceDay.Web` to your hosting platform.
2. Configure API secrets (`Jwt:Key`, DB connection, storage settings).
3. Set `API_BASE_URL` (or `Api:BaseUrl`) on `RaceDay.Web` to the deployed API HTTPS URL.
4. Set API CORS allowed origins to include the deployed Web URL.
5. Verify end-to-end login and event browse/enrol flows.

## Troubleshooting

### `HttpRequestException` / `SocketException` to `localhost:7051`

This means `RaceDay.Web` cannot reach `RaceDay.Api`.

Check:
1. API is running and reachable at configured URL.
2. `API_BASE_URL` / `Api:BaseUrl` points to the correct API endpoint.
3. In production/cloud, do not use localhost for API URL.
4. HTTPS certificate trust (local development).
5. CORS configuration in API includes Web origin.

### API unavailable behavior in Web

If API is down, Home and Events pages fail gracefully with a user-facing message instead of an unhandled exception page.

## Repository structure

```text
RaceDay.sln
src/
  RaceDay.Domain/
  RaceDay.Infrastructure/
  RaceDay.Api/
  RaceDay.Web/
tests/
  RaceDay.Api.Tests/
docs/
  MIGRATION_MAP.md
  RaceDay_API_Endpoint_Plan.md
  RaceDay_Database.sql
  RaceDay_ERD.png
Dockerfile
docker-compose.yml
README.md
```

## Additional documentation

- `docs/MIGRATION_MAP.md`
- `docs/RaceDay_API_Endpoint_Plan.md`
- `docs/RaceDay_Database.sql`
- `docs/RaceDay_ERD.png`
