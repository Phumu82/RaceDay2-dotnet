# Migration Map — React/Supabase → ASP.NET Core / SQL Server

Produced from a full audit of the uploaded `project/` (Vite + React 18 +
TypeScript + React Router + Supabase JS + Tailwind). This is Step 1 of the
brief, done before any code was written.

## Audited inventory of the source app

- **Routes / pages** (`src/pages`): LandingPage, AuthPage, EventsPage, EventDetailPage, AboutPage, ProfilePage, NotFoundPage; `participant/` — ParticipantDashboard, MyEventsPage, MyResultsPage; `organiser/` — OrganiserDashboard, OrganiserEventsPage, EventFormPage, EventManagePage, CategoryManagementPage, EnrolmentManagementPage, ResultManagementPage.
- **Components**: Navbar, Footer, EventCard/EventCardSkeleton/StatCard, ProtectedRoute, `ui/` Button/Card/Input/Badge/Modal/State.
- **State/auth**: `context/AuthContext.tsx` wraps Supabase Auth (`lib/supabase.ts`), exposing `user`, `signIn`, `signUp`, `signOut`.
- **Services** (`src/services`): authService, eventService, categoryService, enrolmentService, resultService, uploadService — each wrapping Supabase `.from(table).select/insert/update/delete()` calls and Supabase Storage for images.
- **Data model** (`src/types/index.ts` + `supabase/migrations/20260828070218_raceday_schema.sql`): `profiles` (role stored directly on the row, not a separate table), `events`, `categories`, `enrolments` (unique on event+participant), `results` (unique on enrolment). Row Level Security policies encode exactly the ownership rules re-implemented in the API's services layer.
- **Validation**: `lib/validation.ts` — client-side only; there was no server-side re-validation beyond Postgres CHECK constraints and RLS.
- **Design system**: `tailwind.config.js` — navy (`#0F1B2D`) + orange accent (`#FF6B35`) palette, Archivo/Inter fonts, rounded-2xl cards.

## Feature-by-feature map

| React/Supabase | ASP.NET Core MVC | Web API | EF Core / DB |
|---|---|---|---|
| `AuthContext` + Supabase Auth | `AccountController` (cookie sign-in, stores JWT as a claim) | `POST /api/auth/register`, `/login`, `/logout` | `Profiles` table, `PasswordHasher<Profile>` |
| `ProtectedRoute` | `[Authorize(Roles=...)]` on MVC controllers + cookie auth redirect | `[Authorize(Roles=...)]` on API controllers + JWT | Role claim signed into JWT at login |
| `EventsPage` (browse/search/filter) | `EventsController.Index` | `GET /api/events` | `EventService.SearchAsync` (LINQ over `Events`) |
| `EventDetailPage` + enrol button | `EventsController.Details` / `Enrol` | `GET /api/events/{id}`, `POST /api/events/{id}/enrolments` | `EnrolmentService.CreateAsync` (unique-index guarded) |
| `ParticipantDashboard` | `ParticipantController.Dashboard` | `GET /api/enrolments/my`, `GET /api/results/my` | aggregated in the MVC controller from API responses |
| `MyEventsPage` / `MyResultsPage` | `ParticipantController.MyEvents` / `MyResults` | same as above | — |
| `ProfilePage` (edit + avatar) | `ProfileController` | `PUT /api/users/profile`, `POST /api/media/avatar` | `ProfileService`, `IBlobStorageService` |
| `OrganiserDashboard` | `OrganiserController.Dashboard` | `GET /api/events/mine` | aggregated from `Events`/`Enrolments` |
| `OrganiserEventsPage` / `EventFormPage` / `EventManagePage` | `OrganiserController.Events/CreateEvent/EditEvent/SaveEvent/DeleteEvent/ManageEvent` | `GET/POST/PUT/DELETE /api/events...` | `EventService` (ownership-checked) |
| `CategoryManagementPage` | `OrganiserController.ManageCategories/SaveCategory/DeleteCategory` | `/api/events/{id}/categories`, `/api/categories/{id}` | `CategoryService` |
| `EnrolmentManagementPage` | `OrganiserController.Enrolments` | `GET /api/events/{id}/enrolments` | `EnrolmentService.GetByEventAsync` |
| `ResultManagementPage` | `OrganiserController.Results/RecordResult/EditResult` | `POST /api/results`, `PUT /api/results/{id}` | `ResultService` |
| `uploadService.ts` (Supabase Storage) | banner/avatar `<input type=file>` forwarded by MVC controllers | `POST /api/media/avatar`, `/api/media/event-banner` | `IBlobStorageService` → Azure Blob Storage (local-disk fallback in dev) |
| Postgres RLS policies | — | ownership checks in each `*Service` (see `RaceDay_API_Endpoint_Plan.md`) | unique indexes/FKs re-implemented in `Data/Configurations/*` and `RaceDay_Database.sql` |

## Deliberate deviations from the brief's suggested entity list

- **No separate `UserRole` table.** The real source schema (`supabase/migrations/20260828070218_raceday_schema.sql`) stores `role` directly on `profiles`, not in a join table. `Profile.Role` mirrors that 1:1 to genuinely preserve the existing data model rather than introduce an unused table the source app never had.
- **`EventType`/`EnrolmentStatus` are C# enums**, stored as strings via `HasConversion<string>()`, matching the source's Postgres `CHECK (... IN (...))` constraints.

## Not carried over (superseded, not silently dropped)

- Supabase Row Level Security policies → replaced by explicit ownership checks in the API services (same rules, enforced in C# instead of Postgres policies) — see Step 18 status in `README.md`.
- Supabase JS client (`lib/supabase.ts`) and `.env` `VITE_SUPABASE_*` keys → replaced by `RaceDay.Web`'s `Api:BaseUrl` config and `RaceDay.Api`'s `ConnectionStrings:RaceDayDb` / `Jwt:*` / `Storage:*` config.
