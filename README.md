# Domain Query Engine

[Demo on Render](https://domain-query-engine-public.onrender.com)

Domain Query Engine is a .NET 8 web application and supporting libraries for querying domain availability and pricing across multiple providers (strategies), aggregating the results, and optionally persisting lookup history to PostgreSQL.

## Solution structure

- `src/DomainQueryEngine.SearchUI` – ASP.NET Core web host (currently uses controllers + views). Provides the UI and an HTTP API endpoint for lookups.
- `src/DomainQueryEngine.Application` – Application services (lookup orchestration, domain search persistence).
- `src/DomainQueryEngine.Core` – Domain entities, models, abstractions (repositories/strategies), utilities.
- `src/DomainQueryEngine.Infrastructure` – Provider-specific lookup strategies and parsers.
- `src/DomainQueryEngine.PostgreSQL` – EF Core PostgreSQL `DbContext`, migrations, repository implementations.
- `tests/*` – Unit test projects.

## Features

- Parallel domain lookup execution via pluggable `IDomainLookupStrategy` implementations
- Aggregated results sorted primarily by availability and then by lowest registration price
- Response caching on lookup endpoint (`VaryByHeader: User-Agent`, 30s)
- Session + cookie-based lightweight tracking (session id / access token)
- Persistence of lookup requests and lookup results to PostgreSQL (EF Core)

## Tech stack

- .NET 8
- ASP.NET Core (controllers/views UI host)
- Entity Framework Core + Npgsql
- Polly for HTTP resiliency (retry)
- Newtonsoft.Json

## Getting started

### Prerequisites

- .NET SDK 8
- A running PostgreSQL instance

### Configure database connection

Update the connection string in `src/DomainQueryEngine.SearchUI/appsettings.json`:

- `ConnectionStrings:DefaultConnection`: `Host=localhost;Database=dqe_db;Username=postgres;Password=...`

### Apply EF Core migrations

From the repository root (or from Visual Studio Package Manager Console), run:

```powershell
# from repo root
cd .\src\DomainQueryEngine.SearchUI

# create the database + apply migrations
# (adjust paths if you run from a different working directory)
dotnet ef database update --project ..\DomainQueryEngine.PostgreSQL --startup-project .
```

### Run the web app

```powershell
cd .\src\DomainQueryEngine.SearchUI
dotnet run
```

Then open the printed URL (for example `https://localhost:xxxx`).

## API

### Domain lookup

`GET /api/lookup/{domain}`

- Example: `GET /api/lookup/example.com`
- Returns: HTML fragment rendered from `Views/Lookup/_Results.cshtml` (content type `text/html`)

Notes:
- The input is normalized/validated before lookup.
- Lookup execution is done in parallel across registered strategies.
- Results are persisted as `DomainLookupResult` records.

## Providers / strategies

Strategies are registered in `src/DomainQueryEngine.Infrastructure/ServiceRegistrations/InfrastructureServiceRegistration.cs` via `AddInfrastructureServices()`.

Current registrations include (names inferred from strategy classes):

- `YildizBilisimLookupStrategy`
- `YoncuBilisimLookupStrategy`
- `GriNetLookupStrategy`
- `AlanAdiKayitLookupStrategy`
- `NatroLookupStrategy`
- `IhsLookupStrategy`
- `IsimKayitLookupStrategy`

## Development notes

- The web host configures resiliency with Polly retry (2 retries, exponential backoff).
- Session is enabled with a 30-minute idle timeout.
- Postgres repositories are wired in `src/DomainQueryEngine.PostgreSQL/ServiceRegistrations/PostgreSQLServiceRegistration.cs`.

## License

Add a license file (for example `LICENSE`) and update this section accordingly.
