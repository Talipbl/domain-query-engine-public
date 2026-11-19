# Domain Query Engine

[Demo on Render](https://domain-query-engine-public.onrender.com)

Domain Query Engine is a .NET 8 web application and supporting libraries for querying domain availability and pricing across multiple providers (strategies), aggregating the results, and optionally persisting lookup history to PostgreSQL.

## Solution structure

- `src/DomainQueryEngine.SearchUI` – ASP.NET Core web host (currently uses controllers + views). Provides the UI and an HTTP API endpoint for lookups.
- `src/DomainQueryEngine.Application` – Application services (lookup orchestration, domain search persistence).
- `src/DomainQueryEngine.Core` – Domain entities, models, abstractions (repositories/strategies), utilities.
- `src/DomainQueryEngine.Infrastructure` – Provider-specific lookup strategies and parsers.
- `src/DomainQueryEngine.PostgreSQL` – EF Core PostgreSQL `DbContext`, migrations, repository implementations.
- `tests/*` – Unit tests and live-site integration tests (see [Tests](#tests)).

## Features

- Parallel domain lookup execution via pluggable `IDomainLookupStrategy` implementations
- Aggregated results sorted by availability, then by lowest registration price in the currency the visitor picks (TRY, USD or EUR)
- Prices are converted with the TCMB daily forex selling rates (refreshed hourly); converted prices are marked "≈" and show the provider's original price. If no rates can be loaded, prices are shown as the providers sent them, TRY first
- The history stores the providers' original prices; changing the display currency is not counted as a new search
- Campaign prices are compared when a provider offers one, otherwise the regular price
- Each provider gets 8 seconds; a slower provider is reported as failed instead of holding up the page
- Results are cached in memory per domain for 5 minutes (1 minute if a provider failed) to limit requests to the providers
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

Credentials are **not** stored in the repository. `appsettings.json` ships with an empty `ConnectionStrings:DefaultConnection`, and the app refuses to start until one is provided.

For local development, use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```powershell
cd .\src\DomainQueryEngine.SearchUI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dqe_db;Username=postgres;Password=<your-password>"
```

For containers / hosting (e.g. Render), set the environment variable instead:

- `ConnectionStrings__DefaultConnection` = `Host=...;Database=...;Username=...;Password=...`

### Apply EF Core migrations

The web app applies pending migrations on startup, so this step is optional.

To run them by hand, use the PostgreSQL project as both the migrations and the startup project: it holds the design-time `AppDbContextFactory` and the `Microsoft.EntityFrameworkCore.Design` package. The factory reads the connection string from the `ConnectionStrings__DefaultConnection` environment variable (User Secrets are not used by `dotnet ef` here).

```powershell
# from repo root
cd .\src\DomainQueryEngine.PostgreSQL

# point EF Core at your database for this shell session
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=dqe_db;Username=postgres;Password=<your-password>"

# create the database + apply migrations
dotnet ef database update
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

- Example: `GET /api/lookup/example.com?currency=USD`
- `currency` (optional): `TRY` (default), `USD` or `EUR`
- `reprice=true` (optional): re-show a search in another currency without saving it to the history
- Returns: HTML fragment rendered from `Views/Lookup/_Results.cshtml` (content type `text/html`)

Notes:
- The input is normalized/validated before lookup.
- Lookup execution is done in parallel across registered strategies.
- Results are persisted as `DomainLookupResult` records.

## Providers / strategies

Strategies are registered in `src/DomainQueryEngine.Infrastructure/ServiceRegistrations/InfrastructureServiceRegistration.cs` via `AddInfrastructureServices()`.

| Provider | Strategy | How it queries | Status |
|---|---|---|---|
| Yıldız Bilişim | `YildizBilisimLookupStrategy` | WHMCS domain check (CSRF token) | Active |
| GriNet | `GriNetLookupStrategy` | WHMCS domain check (CSRF token) | Active |
| Alan Adı Kayıt | `AlanAdiKayitLookupStrategy` | WHMCS domain check (CSRF token) | Active |
| Natro | `NatroLookupStrategy` | JSON query service | Active |
| IHS | `IhsLookupStrategy` | HTML form post | Active |
| Atak Domain | `AtakDomainLookupStrategy` | `POST /api/domain/getavailability` (JSON) | Active |
| Turhost | `TurhostLookupStrategy` | `GET panel.turhost.com/api/v1/domain/query` (JSONP) | Active |
| Turkticaret.net | `TurkticaretLookupStrategy` | `POST smartquery_v2.php` (`trquery` for .tr) | Active |
| Hosting.com.tr | `HostingComTrLookupStrategy` | `POST /web/domain/querynew/lookup` (session + CSRF token) | Active |
| Yöncü Bilişim | `YoncuBilisimLookupStrategy` | HTML endpoint | Registered, currently returns 404 |
| İsim Kayıt | `IsimKayitLookupStrategy` | WHMCS domain check | Registered, currently returns 403 |
| İsim Tescil | `IsimTescilLookupStrategy` | HTML form post | Not registered |
| Güzel Hosting | `GuzelHostingLookupStrategy` | WHMCS `checkAvailabilityOne` | Not registered: Cloudflare answers server-side requests with a bot challenge |

We do not work around bot protection or captchas. When Turkticaret.net asks for its image challenge (`E_KEY_ERROR`), that lookup is reported as failed.

## Tests

Unit tests run offline against provider responses and fake strategies. The only network access they need is a one-time download of the public suffix list.

- `tests/DomainQueryEngine.Infrastructure.UnitTest/Fixtures/Responses/`: real responses captured from Atak Domain, Turhost, Turkticaret.net, Hosting.com.tr and Güzel Hosting
- `tests/DomainQueryEngine.Infrastructure.UnitTest/Fixtures/SampleResponses.cs`: responses for the older providers (Yıldız and Yöncü are real captures; the others were written to match the parsers)

```powershell
# unit tests (default)
dotnet test --filter "Category!=Integration"

# integration tests: send real requests to the provider websites
dotnet test --filter "Category=Integration"
```

Integration tests depend on the providers' live sites and can fail when a site is down, changes its markup or rate-limits requests.

## Development notes

- The web host configures resiliency with Polly retry (2 retries, exponential backoff).
- Session is enabled with a 30-minute idle timeout.
- Postgres repositories are wired in `src/DomainQueryEngine.PostgreSQL/ServiceRegistrations/PostgreSQLServiceRegistration.cs`.
