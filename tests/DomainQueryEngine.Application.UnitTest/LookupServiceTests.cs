using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Application.Test
{
    /// <summary>
    /// Runs LookupService against fake strategies, so no network or database is needed.
    /// </summary>
    public class LookupServiceTests
    {
        private const string Domain = "ornek.com";

        [Fact]
        public async Task Sorts_Available_Then_Priced_Then_Try_Then_Cheapest()
        {
            var service = CreateService(
                Strategy("taken", Result(available: false, 5, "TRY")),
                Strategy("usd-10", Result(available: true, 10, "USD")),
                Strategy("eur-8", Result(available: true, 8, "EUR")),
                Strategy("zero-price", Result(available: true, 0, "TRY")),
                Strategy("try-400", Result(available: true, 400, "TRY")),
                Strategy("try-300", Result(available: true, 300, "TRY")),
                Strategy("no-prices", Result(available: true)));

            var results = await service.LookupDomainAsync(Domain);

            Assert.Equal(
                new[] { "try-300", "try-400", "eur-8", "usd-10", "zero-price", "no-prices", "taken" },
                results.Select(r => r.ProviderInfo.Name));
        }

        [Fact]
        public async Task Uses_Shortest_Period_Price_For_Sorting()
        {
            var cheapLongTerm = Result(available: true);
            cheapLongTerm.Prices.Add(Price(period: 5, 100, "TRY"));
            cheapLongTerm.Prices.Add(Price(period: 1, 900, "TRY"));

            var service = CreateService(
                Strategy("one-year-900", cheapLongTerm),
                Strategy("one-year-500", Result(available: true, 500, "TRY")));

            var results = await service.LookupDomainAsync(Domain);

            Assert.Equal(new[] { "one-year-500", "one-year-900" }, results.Select(r => r.ProviderInfo.Name));
        }

        [Fact]
        public async Task Fills_Visit_Link_Without_Changing_The_Template()
        {
            var templated = Strategy("templated", Result(available: true, 1, "TRY"), "https://provider.test/search?q={0}");
            var fixedLink = Strategy("fixed", Result(available: true, 2, "TRY"), "https://provider.test/domain");
            var service = CreateService(templated, fixedLink);

            var results = await service.LookupDomainAsync(Domain);

            Assert.Equal("https://provider.test/search?q=ornek.com", results.Single(r => r.ProviderInfo.Name == "templated").VisitLink);
            Assert.Equal("https://provider.test/domain", results.Single(r => r.ProviderInfo.Name == "fixed").VisitLink);
            Assert.Equal("https://provider.test/search?q={0}", templated.Provider.VisitLink);
        }

        [Fact]
        public async Task Turns_Strategy_Exception_Into_Failed_Result()
        {
            var service = CreateService(
                Strategy("works", Result(available: true, 1, "TRY")),
                new FakeStrategy("broken", () => throw new HttpRequestException("boom")));

            var results = await service.LookupDomainAsync(Domain);

            var failed = results.Single(r => r.ProviderInfo.Name == "broken");
            Assert.False(failed.Domain.IsSuccess);
            Assert.Equal(Domain, failed.Domain.DomainName);
            Assert.Contains("boom", failed.Domain.RawResponse);
            Assert.True(results.Single(r => r.ProviderInfo.Name == "works").Domain.IsSuccess);
        }

        [Fact]
        public async Task Drops_Strategies_That_Return_No_Result()
        {
            var service = CreateService(
                Strategy("works", Result(available: true, 1, "TRY")),
                new FakeStrategy("empty", () => null));

            var results = await service.LookupDomainAsync(Domain);

            Assert.Equal("works", Assert.Single(results).ProviderInfo.Name);
        }

        [Fact]
        public async Task Measures_Response_Time()
        {
            var slow = new FakeStrategy("slow", () => Result(available: true, 1, "TRY"), delay: TimeSpan.FromMilliseconds(100));
            var service = CreateService(slow);

            var result = Assert.Single(await service.LookupDomainAsync(Domain));

            Assert.InRange(result.ResponseTimeMs, 90, 5000);
        }

        [Fact]
        public async Task Reports_Slow_Provider_As_Timed_Out_Without_Waiting_For_It()
        {
            // The slow strategy ignores the cancellation token, like a strategy calling HttpClient without one.
            var slow = new FakeStrategy("slow", () => Result(available: true, 1, "TRY"), delay: TimeSpan.FromSeconds(10), ignoreToken: true);
            var fast = Strategy("fast", Result(available: true, 2, "TRY"));
            var service = new LookupService(new IDomainLookupStrategy[] { slow, fast }, new NoOpLookupResultRepository(), TimeSpan.FromMilliseconds(200));

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var results = await service.LookupDomainAsync(Domain);

            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3), $"Lookup took {stopwatch.Elapsed}");
            var timedOut = results.Single(r => r.ProviderInfo.Name == "slow");
            Assert.False(timedOut.Domain.IsSuccess);
            Assert.Contains("Timed out", timedOut.Domain.RawResponse);
            Assert.True(results.Single(r => r.ProviderInfo.Name == "fast").Domain.IsSuccess);
        }

        private static LookupService CreateService(params IDomainLookupStrategy[] strategies)
            => new(strategies, new NoOpLookupResultRepository());

        private static FakeStrategy Strategy(string name, DomainResult result, string visitLink = "https://provider.test/{0}")
            => new(name, () => result, visitLink);

        private static DomainResult Result(bool available, decimal? amount = null, string? currency = null)
        {
            var result = new DomainResult { DomainName = Domain, Tld = "com", IsAvailable = available };
            if (amount.HasValue)
                result.Prices.Add(Price(period: 1, amount.Value, currency!));
            return result;
        }

        private static DomainPrice Price(int period, decimal amount, string currency)
            => new() { Period = period, RegistrationPrice = new BasePrice { Amount = amount, Currency = currency } };

        private sealed class FakeStrategy : IDomainLookupStrategy
        {
            private readonly Func<DomainResult?> _lookup;
            private readonly TimeSpan _delay;
            private readonly bool _ignoreToken;

            public FakeStrategy(string name, Func<DomainResult?> lookup, string visitLink = "https://provider.test/{0}", TimeSpan delay = default, bool ignoreToken = false)
            {
                _lookup = lookup;
                _delay = delay;
                _ignoreToken = ignoreToken;
                Provider = new ProviderInfo { Name = name, VisitLink = visitLink };
            }

            public ProviderInfo Provider { get; set; }
            public AuthenticationInfo Authentication { get; set; } = new();

            public Task HandshakeAsync() => Task.CompletedTask;

            public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
            {
                if (_delay > TimeSpan.Zero)
                    await Task.Delay(_delay, _ignoreToken ? CancellationToken.None : ct);
                return _lookup()!;
            }
        }

        private sealed class NoOpLookupResultRepository : IDomainLookupResultRepository
        {
            public Task<DomainLookupResult?> GetByIdAsync(Guid id) => Task.FromResult<DomainLookupResult?>(null);
            public Task<List<DomainLookupResult>> GetAllAsync() => Task.FromResult(new List<DomainLookupResult>());
            public Task<DomainLookupResult> AddAsync(DomainLookupResult entity) => Task.FromResult(entity);
            public Task UpdateAsync(DomainLookupResult entity) => Task.CompletedTask;
            public Task DeleteAsync(DomainLookupResult entity) => Task.CompletedTask;
            public Task<List<DomainLookupResult>> GetByDomainIdAsync(Guid domainId) => GetAllAsync();
            public Task<List<DomainLookupResult>> GetByProviderIdAsync(Guid providerId) => GetAllAsync();
        }
    }
}
