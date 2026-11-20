using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Test
{
    internal sealed class TestCatalogs : ITldPriceCatalog, IProviderDirectory, ISearchStatistics
    {
        private static readonly DateTime CollectedAt = new(2026, 11, 19);
        private readonly List<ProviderEntry> _providers =
        [
            new() { Slug = "turhost", Name = "Turhost", Website = "https://www.turhost.com", Status = ProviderStatus.Active },
            new() { Slug = "natro", Name = "Natro", Website = "https://www.natro.com", Status = ProviderStatus.Active },
            new() { Slug = "turkticaret", Name = "Turkticaret.net", Website = "https://www.turkticaret.net", Status = ProviderStatus.Active }
        ];

        private readonly List<TldEntry> _tlds =
        [
            new()
            {
                Tld = "com",
                Prices =
                [
                    new() { Tld = "com", Provider = "turhost", Currency = "TRY", Registration = 400m, RegistrationTermYears = 1, Renewal = 600m, RenewalTermYears = 1 },
                    new() { Tld = "com", Provider = "natro", Currency = "USD", Registration = 2.99m, Renewal = 22.99m }
                ]
            },
            new()
            {
                Tld = "com.tr",
                Prices =
                [
                    new() { Tld = "com.tr", Provider = "turhost", Currency = "TRY", Registration = 100m, Renewal = 800m, RenewalTermYears = 1 },
                    new() { Tld = "com.tr", Provider = "natro", Currency = "USD", Registration = 1.49m, RegularRegistration = 19.99m, Renewal = 19.99m }
                ]
            }
        ];

        public Task<DateTime> GetAsOfAsync(CancellationToken ct = default) => Task.FromResult(CollectedAt);
        Task<IReadOnlyList<TldEntry>> ITldPriceCatalog.GetAllAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<TldEntry>>(_tlds);

        Task<TldEntry?> ITldPriceCatalog.GetAsync(string tld, CancellationToken ct)
        {
            var key = tld.Trim().TrimStart('.').Replace('-', '.');
            return Task.FromResult(_tlds.FirstOrDefault(t => string.Equals(t.Tld, key, StringComparison.OrdinalIgnoreCase)));
        }

        Task<IReadOnlyList<ProviderEntry>> IProviderDirectory.GetAllAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ProviderEntry>>(_providers);

        Task<ProviderEntry?> IProviderDirectory.GetAsync(string slug, CancellationToken ct)
            => Task.FromResult(_providers.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<PopularQuery>> GetMostSearchedAsync(TimeSpan window, int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PopularQuery>>(Array.Empty<PopularQuery>());

        public Task<IReadOnlyList<PopularTld>> GetMostSearchedTldsAsync(TimeSpan window, int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PopularTld>>(
                new[] { new PopularTld("com.tr", 12), new PopularTld("com", 8) }.Take(take).ToList());
    }
}
