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
| TLD prices | PostgreSQL | Current registration and renewal price snapshots; no sample fallback is used |
| Provider profiles | Configured price-list strategies | Provider names and websites are derived from the live source adapters |
| Search statistics | PostgreSQL | "Most searched" list from lookup history |
| `PriceCollection:IntervalMinutes` / `PriceCollection:MaxConcurrency` | `360` / `3` | Scheduled provider price-list collection cadence and maximum parallel fetches |
| `Database:MigrateOnStartup` | `true` | Apply pending migrations when the app starts |

Environment variables use `__` instead of `:` (e.g. `Site__BaseUrl`).

### Container

The `Dockerfile` builds and runs the web app. At runtime it needs at least `ConnectionStrings__DefaultConnection`; set `Site__BaseUrl` to the public address so canonical links and the sitemap use `https`. Texts and number formats follow the chosen language (`tr-TR` or `en-US`), so the image needs ICU (the Debian-based `aspnet` images include it; Alpine images do not).

## UI

Pages are rendered on the server; htmx loads search results and makes filters, tabs and sorting update without full page loads. Everything also works as plain links and forms.

| Address | Page | Data |
|---|---|---|
| `/` | Search box, cheapest offers for popular extensions, most searched extensions in the last 24 hours, providers | Provider price snapshots, statistics |
| `/ara?q=…` | Search results: page renders at once, results load from `/ara/sonuclar`. Filters: only available, period (1/2/5 years), sort | Live lookups |
| `/uzantilar` | Extension list: text filter, sort by registration price, renewal price or name | Provider price snapshots |
| `/uzantilar/{slug}` | One extension (`/uzantilar/com-tr`): current registration and renewal prices by provider | Provider price snapshots |
| `/saglayicilar`, `/saglayicilar/{slug}` | Providers with their status; one provider's registration and renewal prices | Provider profiles, price snapshots |
| `/gizlilik` | Privacy | Static |
| `/_bilesenler` | Component gallery (Development only) | Static examples |

Filter, sort and currency changes on `/ara` re-show the cached results and are not counted as new searches.

- **Design system:** `wwwroot/css/tokens.css` (colors, type, spacing; light and dark themes), `base.css`, `components.css`.
- **Theme:** the sun/moon button in the header switches light/dark at once and saves the choice in the `dqe_theme` cookie; the server renders `data-theme` from it, so the page does not flash. Without a choice the page follows the system setting.
- **Components:** tag helpers `<badge>`, `<btn>`, `<price-tag>` (`TagHelpers/`) and partials under `Views/Shared` and `Views/Shared/Components` (`_ResultRow`, `_SearchResults`, `_Offer`, `_DataNote`, …).
- **Currency:** chosen in the header, stored in the `dqe_currency` cookie; the form works without JavaScript.
- **Languages:** Turkish (default) and English, chosen with TR/EN in the header and kept in the ASP.NET culture cookie (`?culture=en-US` also works). All texts are key/value pairs in `Resources/Locales/tr.json` and `en.json`, embedded in the assembly and read by `Localization/JsonStringLocalizer.cs`; views use them through `IStringLocalizer<SharedResource>` (`@T["Nav.Search"]`, with arguments `@T["Results.Failed", count]`). A key ending in `.One` is used instead when the first argument is 1 (`"1 search"`). A key missing in English falls back to Turkish and is logged. Extension descriptions and FAQs in the catalog have English fields (`descriptionEn`, `questionEn`, `answerEn`). Tests check that both files have the same keys and that every key used in the code exists.
- **To add a language:** add `Resources/Locales/{code}.json` with the same keys and the culture to `LanguagePreference.Languages`.
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
| Yöncü Bilişim | `YoncuBilisimLookupStrategy` | JSON API (`/API/Domain/Check`, cftoken) | Registered |
| İsim Kayıt | `IsimKayitLookupStrategy` | WHMCS domain check | Not registered: the site left WHMCS, the check endpoint returns 403 and there is no server-side availability API |
| İsim Tescil | `IsimTescilLookupStrategy` | HTML form post | Not registered |
| Güzel Hosting | `GuzelHostingLookupStrategy` | WHMCS `checkAvailabilityOne` | Not registered: Cloudflare answers server-side requests with a bot challenge |

We do not work around bot protection or captchas. When Turkticaret.net asks for its image challenge (`E_KEY_ERROR`), that lookup is reported as failed.

## Tests

Unit tests run offline against provider responses and fake strategies. The only network access they need is a one-time download of the public suffix list.

- `tests/DomainQueryEngine.Infrastructure.UnitTest/Fixtures/Responses/`: real responses captured from Atak Domain, Turhost, Turkticaret.net, Hosting.com.tr and Güzel Hosting
- `tests/DomainQueryEngine.Infrastructure.UnitTest/Fixtures/SampleResponses.cs`: responses for the older providers (Yıldız is a real capture; the others were written to match the parsers)
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
