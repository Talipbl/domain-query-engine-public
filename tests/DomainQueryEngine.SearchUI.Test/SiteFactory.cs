using DomainQueryEngine.Application;
using DomainQueryEngine.Application.DomainSearch;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using DomainQueryEngine.SearchUI.Hosting.TldPricing;

namespace DomainQueryEngine.SearchUI.Test
{
    /// <summary>
    /// Starts the real app with external dependencies replaced. Catalogs return a small,
    /// deterministic set of test data.
    /// </summary>
    public sealed class SiteFactory : WebApplicationFactory<Program>
    {
        public RecordingSearches Searches { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=unused-in-tests");
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting("Site:BaseUrl", "https://test.example");

            builder.ConfigureServices(services =>
            {
                foreach (var descriptor in services.Where(d => d.ServiceType == typeof(IHostedService)
                    && d.ImplementationType == typeof(TldPriceCollectionHostedService)).ToList())
                    services.Remove(descriptor);
                var catalogs = new TestCatalogs();
                services.RemoveAll<ITldPriceCatalog>();
                services.AddSingleton<ITldPriceCatalog>(catalogs);
                services.RemoveAll<IProviderDirectory>();
                services.AddSingleton<IProviderDirectory>(catalogs);
                services.RemoveAll<ISearchStatistics>();
                services.AddSingleton<ISearchStatistics>(catalogs);

                services.RemoveAll<IExchangeRateProvider>();
                services.AddSingleton<IExchangeRateProvider>(new FixedRates());

                services.RemoveAll<ILookupService>();
                services.AddScoped<ILookupService, FakeLookupService>();

                services.RemoveAll<IDomainService>();
                services.RemoveAll<IProviderService>();
                services.AddSingleton(Searches);
                services.AddSingleton<IDomainService>(sp => sp.GetRequiredService<RecordingSearches>());
                services.AddSingleton<IProviderService>(sp => sp.GetRequiredService<RecordingSearches>());
            });
        }

        private sealed class FixedRates : IExchangeRateProvider
        {
            private static readonly ExchangeRates Rates = new(new DateTime(2026, 11, 19), "TCMB",
                new Dictionary<string, decimal> { ["USD"] = 50m, ["EUR"] = 55m });

            public Task<ExchangeRates?> GetRatesAsync(CancellationToken ct = default) => Task.FromResult<ExchangeRates?>(Rates);
        }

        /// <summary>Two providers answer (one in TRY, one in USD with a campaign), one fails.</summary>
        private sealed class FakeLookupService : ILookupService
        {
            public Task<List<SearchResult>> LookupDomainAsync(string domain, CancellationToken ct = default)
            {
                var results = new List<SearchResult>
                {
                    Result("Lira Host", domain, available: true, new DomainPrice { Period = 1, RegistrationPrice = Price(300m, "TRY") }),
                    Result("Dolar Host", domain, available: true, new DomainPrice { Period = 1, RegistrationPrice = Price(5m, "USD"), RegularRegistrationPrice = Price(9m, "USD") }),
                    new()
                    {
                        ProviderInfo = new ProviderInfo { Name = "Hatalı Host" },
                        Domain = new DomainResult { DomainName = domain, IsSuccess = false, RawResponse = "Error: Response status code does not indicate success: 404 (Not Found)." }
                    }
                };
                return Task.FromResult(results);
            }

            public Task AddDomainLookupResult(DomainLookupResult lookupResult) => Task.CompletedTask;

            private static SearchResult Result(string provider, string domain, bool available, DomainPrice price)
                => new()
                {
                    ProviderInfo = new ProviderInfo { Name = provider, VisitLink = "https://provider.test/{0}" },
                    VisitLink = $"https://provider.test/{domain}",
                    Domain = new DomainResult { DomainName = domain, Tld = "com", IsAvailable = available, Prices = { price } }
                };

            private static BasePrice Price(decimal amount, string currency) => new() { Amount = amount, Currency = currency };
        }
    }

    /// <summary>Counts searches written to the history instead of saving them to a database.</summary>
    public sealed class RecordingSearches : IDomainService, IProviderService
    {
        private readonly List<string> _domains = new();

        public IReadOnlyList<string> Domains
        {
            get { lock (_domains) return _domains.ToList(); }
        }

        public Task<Domain> AddDomainSearch(Domain domain)
        {
            lock (_domains) _domains.Add(domain.DomainName);
            return Task.FromResult(domain);
        }

        public Task<Provider> GetOrCreateProviderAsync(ProviderInfo info)
            => Task.FromResult(new Provider { Name = info.Name });
    }
}
