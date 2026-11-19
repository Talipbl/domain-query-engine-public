using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using DomainQueryEngine.GriNet;
using DomainQueryEngine.YildizBilisim;
using DomainQueryEngine.YoncuBilisim;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using HttpClientFactory = DomainQueryEngine.Http.HttpClientFactory;
using IHttpClientFactory = DomainQueryEngine.Http.IHttpClientFactory;

namespace DomainQueryEngine.Application.Test
{
    public class TestBase
    {
        private ServiceProvider _services;
        protected ILookupService LookupService => _services.GetRequiredService<ILookupService>();

        public TestBase()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(Policy.NoOpAsync<HttpResponseMessage>());
            services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
            services.AddSingleton<IDomainLookupResultRepository, InMemoryDomainLookupResultRepository>();

            services.AddSingleton<IDomainLookupStrategy, YildizBilisimLookupStrategy>();
            services.AddSingleton<IDomainLookupStrategy, YoncuBilisimLookupStrategy>();
            services.AddSingleton<IDomainLookupStrategy, GriNetLookupStrategy>();
            services.AddSingleton<YildizBilisimStrategyParser>();
            services.AddSingleton<YoncuBilisimStrategyParser>();
            services.AddSingleton<GriNetStrategyParser>();
            services.AddSingleton<ILookupService, LookupService>();

            _services = services.BuildServiceProvider();
        }

        /// <summary>
        /// Keeps lookup results in memory so tests do not need a database.
        /// </summary>
        private sealed class InMemoryDomainLookupResultRepository : IDomainLookupResultRepository
        {
            private readonly List<DomainLookupResult> _items = new();

            public Task<DomainLookupResult?> GetByIdAsync(Guid id)
                => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

            public Task<List<DomainLookupResult>> GetAllAsync()
                => Task.FromResult(_items.ToList());

            public Task<DomainLookupResult> AddAsync(DomainLookupResult entity)
            {
                _items.Add(entity);
                return Task.FromResult(entity);
            }

            public void Update(DomainLookupResult entity) { }

            public void Delete(DomainLookupResult entity) => _items.Remove(entity);

            public Task<List<DomainLookupResult>> GetByDomainIdAsync(Guid domainId)
                => Task.FromResult(_items.Where(i => i.DomainId == domainId).ToList());

            public Task<List<DomainLookupResult>> GetByProviderIdAsync(Guid providerId)
                => Task.FromResult(_items.Where(i => i.ProviderId == providerId).ToList());
        }
    }
}
