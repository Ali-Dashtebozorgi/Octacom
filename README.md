# Octacom — Mini Conference Ticket Booking API

A backend-only .NET 8 Web API implementing a conference seat booking system, built with Onion Architecture, Domain-Driven Design, and Test-Driven Development.

## Tech Stack

- **.NET 8** / ASP.NET Core Web API
- **EF Core** with SQL Server (LocalDB)
- **Serilog** for structured file logging
- **xUnit**, **Moq**, **FluentAssertions** for testing
- **Swagger / OpenAPI** for API documentation

## Architecture

The solution follows **Onion Architecture** with the domain at the center, and was built test-first (TDD) throughout.

```
Octacom.sln
├── src/
│   ├── Octacom.Domain          # Entities, value objects, domain logic, exceptions
│   ├── Octacom.Application     # Use cases, DTOs, service interfaces
│   ├── Octacom.Infrastructure  # EF Core, repositories, database seeding
│   └── Octacom.API             # Controllers, middleware, Swagger, Serilog
└── tests/
    ├── Octacom.UnitTests        # Domain + Application layer tests
    └── Octacom.IntegrationTests # Full HTTP pipeline + real database tests
```

**Key design decisions:**

- `Conference` is the **Aggregate Root**; `Booking` is an Entity that only exists within it. All persistence goes through `IConferenceRepository` — there is no separate booking repository, since bookings are never accessed independently of their conference.
- `Email` is implemented as a **Value Object** with built-in format validation.
- Business rules (capacity checks, duplicate booking prevention, waitlist promotion) live entirely inside the `Conference` aggregate — the Application layer simply orchestrates loading, calling domain behavior, and persisting. This kept the Application layer untouched even when new features like the waitlist were added later.
- **Optimistic concurrency control** is implemented via EF Core's `RowVersion`, ensuring two simultaneous booking attempts for the last seat can't both succeed.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio, or installable via [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb))

### Clone and Run

```bash
git clone https://github.com/Ali-Dashtebozorgi/Octacom.git
cd Octacom

dotnet build

dotnet run --project src/Octacom.API
```

That's it — no manual database setup required. On startup, the application automatically:
1. Creates the database if it doesn't exist
2. Applies all EF Core migrations
3. Seeds a default conference (100 seats) if none exists

The API will be available at `https://localhost:7230`, with Swagger UI at `https://localhost:7230/swagger`.

### Connection String

The default connection string is in `src/Octacom.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=.;Initial Catalog=OctacomConferenceDb;Integrated Security=true;TrustServerCertificate=Yes;"
}
```

Adjust if your LocalDB instance name differs.

### Logs

Request/response logs and application logs are written to `logs/octacom-{date}.log` (rolling daily), as well as the console.

## API Endpoints

All responses follow a consistent envelope:

```json
{
  "success": true,
  "data": { },
  "error": null
}
```

### Conferences

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/conferences` | Create a new conference |
| `GET` | `/api/conferences?page=1&pageSize=10` | List all conferences (paginated) |
| `GET` | `/api/conferences/{id}` | Get a single conference's details |
| `GET` | `/api/conferences/{id}/bookings?page=1&pageSize=10` | List bookings for a conference (paginated) |

**Create a conference**

```http
POST /api/conferences
Content-Type: application/json

{
  "name": "Tech Summit 2026",
  "totalCapacity": 50
}
```

### Bookings

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/conferences/{conferenceId}/bookings` | Book a seat |
| `GET` | `/api/conferences/{conferenceId}/bookings/{id}` | Get a single booking |
| `DELETE` | `/api/conferences/{conferenceId}/bookings/{id}` | Cancel a booking |

**Book a seat**

```http
POST /api/conferences/{conferenceId}/bookings
Content-Type: application/json

{
  "attendeeName": "Ali Dasht",
  "attendeeEmail": "ali@test.com"
}
```

If the conference is full, the booking is automatically **waitlisted** rather than rejected (see Features below).

**Cancel a booking**

```http
DELETE /api/conferences/{conferenceId}/bookings/{id}
```

If a confirmed seat is cancelled and someone is waitlisted, the earliest waitlisted booking is **automatically promoted** to confirmed.

### Postman Collection

A Postman collection covering all endpoints, including edge cases (full conference, duplicate email, invalid input, not-found scenarios), is included at `Octacom.postman_collection.json`.

## Features Beyond the Base Requirements

- **Waitlist with auto-promotion** — bookings against a full conference are waitlisted instead of rejected; cancelling a confirmed seat automatically promotes the earliest waitlisted attendee.
- **Human-friendly confirmation codes** — each booking gets a short code (e.g. `OCT-7F3A2`) in addition to its GUID.
- **Pagination** — `GET /api/conferences` and `GET /api/conferences/{id}/bookings` support `page`/`pageSize` query parameters.
- **Optimistic concurrency** — prevents double-booking the last seat under concurrent requests, verified with a dedicated test (see below).
- **Request/response logging with correlation IDs** — every request is logged with a unique correlation ID (also returned as an `X-Correlation-Id` response header), so a full request lifecycle can be traced through the logs even under concurrent traffic.
- **Centralized exception handling** — a single middleware maps domain exceptions to appropriate HTTP status codes and a consistent error response shape, so no controller needs its own try/catch.

## Testing

The system was built test-first throughout (TDD), with two levels of testing:

### Unit Tests (`Octacom.UnitTests`)

Covers the Domain and Application layers in isolation (repository mocked via Moq). Includes:
- Entity behavior and invariants (`Conference`, `Booking`)
- Value object validation and equality (`Email`)
- Waitlist and promotion logic
- Confirmation code generation
- Application service orchestration and pagination logic

Run with:
```bash
dotnet test tests/Octacom.UnitTests
```

### Integration Tests (`Octacom.IntegrationTests`)

Runs the full HTTP pipeline (`WebApplicationFactory`) against a real LocalDB test database, covering:
- Full request/response cycles for every endpoint, including error paths (400/404/409)
- The complete waitlist → cancellation → promotion flow end-to-end
- **A deterministic optimistic concurrency test**, simulating two concurrent requests loading the same conference and proving only one can successfully book the last seat — the second receives a `DbUpdateConcurrencyException`, mapped to a `409 Conflict`

Run with:
```bash
dotnet test tests/Octacom.IntegrationTests
```

> Integration tests run sequentially against a shared LocalDB database, so test parallelization is disabled at the assembly level to avoid cross-test data collisions.

Run everything:
```bash
dotnet test
```

## Known Trade-offs / What I'd Add With More Time

These are deliberate scope decisions made for a time-boxed exercise, not oversights:

- **Confirmation code generation** currently lives inside the `Booking` entity for simplicity. In a production system I'd extract it into an injectable `IConfirmationCodeGenerator` service to keep the domain free of infrastructure/randomness concerns and to make it independently testable.
- **Service-level business event logging** (e.g. "booking attempt started", "seat full, waitlisting") was intentionally left out to keep scope focused — currently logging exists at the request/response and exception-handling boundaries only.
- **Idempotency keys** on the booking endpoint would be a natural next step for production resilience against retried requests.
- Integration tests share one LocalDB database and run sequentially; in a CI pipeline I'd move to **Testcontainers** for fully isolated, parallelizable test runs.
