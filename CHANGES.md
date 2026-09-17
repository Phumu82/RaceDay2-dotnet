# Changes in this PR

## Summary
This pull request adds API resilience handling and cloud-ready configuration to RaceDay.Web, plus comprehensive documentation improvements.

## Changes

### 1. API Error Resilience (RaceDay.Web)

#### HomeController.cs
- Added `ILogger<HomeController>` dependency injection
- Wrapped `SearchEventsAsync` call in try-catch for `HttpRequestException`
- When API is unavailable, logs warning and displays user-friendly error message
- Returns empty event list instead of crashing with unhandled exception

#### EventsController.cs
- Added `ILogger<EventsController>` dependency injection
- Wrapped `SearchEventsAsync` in `Index()` with connection error handling
- Wrapped API calls in `Details()` with connection error handling
- Graceful fallback: redirects with TempData error message instead of throwing
- Both methods catch `HttpRequestException` specifically

### 2. Cloud-Ready Configuration (RaceDay.Web)

#### Program.cs
**Added:**
- `API_BASE_URL` environment variable support (preferred for cloud deployments)
- Absolute URL validation using `Uri.TryCreate()`
- Production guard: rejects localhost/loopback API URLs outside Development environment
- Clear error messages for configuration issues

**Before:**
```csharp
var baseUrl = builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException(...);
client.BaseAddress = new Uri(baseUrl);
```

**After:**
```csharp
var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(...);

if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiUri))
    throw new InvalidOperationException($"Api:BaseUrl is not a valid absolute URL: '{baseUrl}'.");

if (!builder.Environment.IsDevelopment() && apiUri.IsLoopback)
    throw new InvalidOperationException("Production requires a non-local API endpoint...");

client.BaseAddress = apiUri;
```

#### appsettings.json
- Changed default `Api:BaseUrl` from `https://localhost:7051/` to `https://your-raceday-api-host/`
- Forces explicit cloud configuration, preventing accidental localhost deployments

#### appsettings.Development.json
- Keeps localhost configuration for local development: `https://localhost:7051`

### 3. Documentation (README.md)

Complete rewrite with:
- **Clear project description** and what it does
- **Solution architecture table** for each project
- **Tech stack** listing .NET 10, ASP.NET Core, EF Core, JWT, xUnit, Docker
- **Prerequisites** section with download links
- **Configuration reference** with tables for Api and Web settings
- **Local development quick start** with copy-paste commands
- **Database options** section explaining migrations and SQL scripts
- **Testing and Docker** instructions
- **Cloud deployment checklist** (5 key steps)
- **Troubleshooting** section for `HttpRequestException` and API unavailable behavior
- **Repository structure** and links to additional documentation

## Testing

### Local Development
1. Set `Jwt:Key` secret via user-secrets
2. Run both API and Web projects
3. API down: Home/Events pages show error message instead of exception

### Cloud Deployment
1. Set `API_BASE_URL` environment variable on Web deployment
2. If set to localhost outside Development: immediate startup error (safe-fail)
3. If set to valid cloud URL: Web connects to API over HTTPS

### Build
```bash
dotnet build RaceDay.sln  # Passes ✓
```

## Backward Compatibility
✓ Fully backward compatible
- Development `appsettings.Development.json` unchanged (uses localhost)
- Fallback to `Api:BaseUrl` if `API_BASE_URL` not set
- Error handling is additive (existing success paths unchanged)

## Breaking Changes
❌ None

## Deployment Checklist
- [ ] Test locally with API running
- [ ] Test locally with API stopped (expect graceful errors)
- [ ] Deploy to cloud with `API_BASE_URL` set to cloud API URL
- [ ] Verify login and event browse/enrol flows end-to-end
- [ ] Check logs for any warnings

## Related
- Fixes: Socket/connection refused errors now fail gracefully
- Improves: Cloud readiness with environment-based configuration
- Enhances: Developer experience with clear documentation
