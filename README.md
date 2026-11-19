# Domain Query Engine

[Demo on Render](https://domain-query-engine-public.onrender.com)

Domain Query Engine is a .NET 8 web application and supporting libraries for querying domain availability and pricing across multiple providers (strategies), aggregating the results, and optionally persisting lookup history to PostgreSQL.

## Solution structure

- `src/DomainQueryEngine.SearchUI` – ASP.NET Core web host: server-rendered Razor pages enhanced with htmx (see [UI](#ui)), plus the lookup API.
- `src/DomainQueryEngine.Application` – Application services (lookup orchestration, price comparison and currency conversion, extension price pages, domain search persistence).
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
- ASP.NET Core MVC (Razor views, tag helpers) with [htmx](https://htmx.org) 2.0.4 (`wwwroot/lib/htmx`, 0BSD)
- Plain CSS design system (no CSS framework, no frontend build step)
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

### Configuration

| Setting | Default | Meaning |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | empty (required) | PostgreSQL connection string |
| `Site:BaseUrl` | empty | Public address used for canonical links, Open Graph and the sitemap (e.g. `https://example.com`). Empty: taken from the request; set it behind a proxy that terminates HTTPS |
| `DataSource:Tlds` / `DataSource:Providers` | `Mock` | Extension and provider catalog. Only `Mock` (embedded sample data) exists so far |
| `DataSource:Statistics` | `Database` | "Most searched" list: `Database` (lookup history) or `Mock` |
| `Database:MigrateOnStartup` | `true` | Apply pending migrations when the app starts |

Environment variables use `__` instead of `:` (e.g. `Site__BaseUrl`).

### Container

The `Dockerfile` builds and runs the web app. At runtime it needs at least `ConnectionStrings__DefaultConnection`; set `Site__BaseUrl` to the public address so canonical links and the sitemap use `https`. Texts and number formats are fixed to Turkish (`tr-TR`) by the app, so the image needs ICU (the Debian-based `aspnet` images include it; Alpine images do not).

## UI

Pages are rendered on the server; htmx loads search results and makes filters, tabs and sorting update without full page loads. Everything also works as plain links and forms.

| Address | Page | Data |
|---|---|---|
| `/` | Search box, cheapest offers for popular extensions, most searched domains, providers | Catalog (sample), statistics |
| `/ara?q=…` | Search results: page renders at once, results load from `/ara/sonuclar`. Filters: only available, period (1/2/5 years), sort | Live lookups |
| `/uzantilar` | Extension list: group tabs, text filter, sort by popularity, cheapest registration, cheapest renewal or name | Catalog (sample) |
| `/uzantilar/{slug}` | One extension (`/uzantilar/com-tr`): summary, per-provider prices, price history, FAQ | Catalog (sample) |
| `/saglayicilar`, `/saglayicilar/{slug}` | Providers with their status; one provider's prices | Catalog (sample) |
| `/gizlilik` | Privacy | Static |
| `/_bilesenler` | Component gallery (Development only) | Sample |

Sample data is labelled "Örnek veri" on the pages. Filter, sort and currency changes on `/ara` re-show the cached results and are not counted as new searches.

- **Design system:** `wwwroot/css/tokens.css` (colors, type, spacing; dark theme follows the system setting), `base.css`, `components.css`.
- **Components:** tag helpers `<badge>`, `<btn>`, `<price-tag>` (`TagHelpers/`) and partials under `Views/Shared` and `Views/Shared/Components` (`_ResultRow`, `_SearchResults`, `_Offer`, `_DataNote`, …).
- **Currency:** chosen in the header, stored in the `dqe_currency` cookie; the form works without JavaScript.
- **Texts:** shared texts live in `Resources/SharedResource.tr.resx` and are used through `IStringLocalizer<SharedResource>` (`@T["Nav.Search"]`). Only Turkish exists; to add a language, add `SharedResource.{culture}.resx` and the culture in `Program.cs`. Page-specific texts are still written in the views.
- **SEO:** canonical and Open Graph tags on catalog pages, `noindex` on search pages, `/sitemap.xml` and `/robots.txt`.

## API

### Domain lookup

`GET /api/lookup/{domain}`

- Example: `GET /api/lookup/example.com?currency=USD`
- `currency` (optional): `TRY` (default), `USD` or `EUR`
- `reprice=true` (optional): re-show a search in another currency without saving it to the history
- Returns: HTML fragment rendered from `Views/Shared/Components/_SearchResults.cshtml` (content type `text/html`)
- The site's own pages use `/ara` and `/ara/sonuclar`; this endpoint stays for existing clients.

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
- `tests/DomainQueryEngine.SearchUI.Test`: page tests that start the whole app with `WebApplicationFactory`. The database, provider sites and TCMB are replaced by fakes, so they need neither PostgreSQL nor network access

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
