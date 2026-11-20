using DomainQueryEngine.Application.DomainSearch;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace DomainQueryEngine.Application.Test
{
    public class CachedLookupServiceTests
    {
        [Fact]
        public async Task Second_Search_For_Same_Domain_Does_Not_Call_Providers_Again()
        {
            var strategy = new CountingStrategy(success: true);
            var service = Create(strategy);

            await service.LookupDomainAsync("ornek.com");
            var second = await service.LookupDomainAsync("ORNEK.com");

            Assert.Equal(1, strategy.Calls);
            Assert.Single(second);
        }

        [Fact]
        public async Task Different_Domains_Are_Cached_Separately()
        {
            var strategy = new CountingStrategy(success: true);
            var service = Create(strategy);

            await service.LookupDomainAsync("ornek.com");
            await service.LookupDomainAsync("baska.com");

            Assert.Equal(2, strategy.Calls);
        }

        [Fact]
        public async Task Returns_A_New_List_Each_Time()
        {
            var service = Create(new CountingStrategy(success: true));

            var first = await service.LookupDomainAsync("ornek.com");
            first.Clear();
            var second = await service.LookupDomainAsync("ornek.com");

            Assert.Single(second);
        }

        private static CachedLookupService Create(IDomainAvailabilityProvider strategy)
            => new(new LookupService(new[] { strategy }, new NullRepository()), new MemoryCache(new MemoryCacheOptions()));

        private sealed class CountingStrategy : IDomainAvailabilityProvider
        {
            private readonly bool _success;
            public CountingStrategy(bool success) => _success = success;

            public int Calls { get; private set; }
            public ProviderInfo Provider { get; set; } = new() { Name = "counting", VisitLink = "https://provider.test/{0}" };
            public AuthenticationInfo Authentication { get; set; } = new();
            public Task HandshakeAsync() => Task.CompletedTask;

            public Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
            {
                Calls++;
                return Task.FromResult(new DomainResult { DomainName = domainName, IsSuccess = _success, IsAvailable = true });
            }
        }

        private sealed class NullRepository : IDomainLookupResultRepository
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
