# RaceDay API — Endpoint Plan

Base URL (dev): `https://localhost:7051/`
Swagger UI (dev only): `https://localhost:7051/swagger`

All endpoints return JSON. Authenticated endpoints require:
`Authorization: Bearer <jwt>`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| POST | /api/auth/register | Anonymous | — | Create account, returns JWT + profile |
| POST | /api/auth/login | Anonymous | — | Returns JWT + profile |
| POST | /api/auth/logout | Anonymous | — | Stateless no-op (client discards JWT) |
| GET | /api/users/profile | Bearer | Any | Current user's profile |
| PUT | /api/users/profile | Bearer | Any | Update full name |
| POST | /api/media/avatar | Bearer | Any | Upload avatar image, updates profile |
| GET | /api/events | Anonymous | — | Search/browse events (search, type, location, from, to) |
| GET | /api/events/{id} | Anonymous | — | Event details |
| GET | /api/events/mine | Bearer | Organiser | Events the caller organises |
| POST | /api/events | Bearer | Organiser | Create event |
| PUT | /api/events/{id} | Bearer | Organiser (owner) | Update event |
| DELETE | /api/events/{id} | Bearer | Organiser (owner) | Delete event |
| POST | /api/media/event-banner | Bearer | Organiser | Upload banner image, returns URL |
| GET | /api/events/{eventId}/categories | Anonymous | — | List categories for an event |
| POST | /api/events/{eventId}/categories | Bearer | Organiser (owner) | Create category |
| PUT | /api/categories/{id} | Bearer | Organiser (owner) | Update category |
| DELETE | /api/categories/{id} | Bearer | Organiser (owner) | Delete category (blocked if enrolments exist) |
| POST | /api/events/{eventId}/enrolments | Bearer | Participant | Enrol in an event/category (409 if duplicate) |
| GET | /api/enrolments/my | Bearer | Participant | Caller's own enrolments |
| GET | /api/events/{eventId}/enrolments | Bearer | Organiser (owner) | Enrolments for an event |
| POST | /api/results | Bearer | Organiser (owner) | Record a result for an enrolment |
| PUT | /api/results/{id} | Bearer | Organiser (owner) | Edit a result |
| GET | /api/results/my | Bearer | Participant | Caller's own results |
| GET | /api/events/{eventId}/results | Bearer | Organiser (owner) | All results for an event |

## Status codes

- `200 OK` / `201 Created` — success
- `400 Bad Request` — validation failure (model binding, FluentValidation-style DataAnnotations)
- `401 Unauthorized` — missing/invalid/expired JWT
- `403 Forbidden` — authenticated, but wrong role or not the resource owner
- `404 Not Found` — entity does not exist
- `409 Conflict` — duplicate email, duplicate enrolment, duplicate result, category with active enrolments
- `500 Internal Server Error` — unhandled exception (logged; message never leaks internals — see `ExceptionHandlingMiddleware`)

## Ownership rules enforced server-side (not trusted from the client)

- An Organiser can only update/delete their **own** events, categories, enrolment views, and results — enforced in each service method by comparing `OrganiserId`/`Event.OrganiserId` against the JWT's `NameIdentifier` claim, never from a request body field.
- A Participant can only see and create enrolments/results tied to their own `ParticipantId`, taken from the JWT — never from a client-supplied `participantId`.
- Role checks use `[Authorize(Roles = "Organiser")]` / `[Authorize(Roles = "Participant")]` at the controller-action level, backed by the `Role` claim signed into the JWT at login — a client cannot forge this without the server's signing key.
