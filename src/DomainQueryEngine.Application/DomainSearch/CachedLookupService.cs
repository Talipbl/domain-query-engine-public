using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace DomainQueryEngine.Application.DomainSearch
{
    /// <summary>
    /// Keeps lookup results in memory for a short time so repeated searches for the same domain do not
    /// send another request to every provider (which also keeps us under the providers' rate limits).
    /// </summary>
    public class CachedLookupService : ILookupService
    {
        public static readonly TimeSpan SuccessDuration = TimeSpan.FromMinutes(5);

        /// <summary>Shorter when a provider failed, so a temporary error does not stick for long.</summary>
        public static readonly TimeSpan PartialFailureDuration = TimeSpan.FromMinutes(1);

        private readonly LookupService _inner;
        private readonly IMemoryCache _cache;

        public CachedLookupService(LookupService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public Task AddDomainLookupResult(DomainLookupResult lookupResult)
            => _inner.AddDomainLookupResult(lookupResult);

        public async Task<List<SearchResult>> LookupDomainAsync(string domain, CancellationToken ct = default)
        {
            var key = $"lookup:{domain.ToLowerInvariant()}";
            if (_cache.TryGetValue(key, out List<SearchResult>? cached) && cached != null)
                return cached.ToList();

            var results = await _inner.LookupDomainAsync(domain, ct);

            var anyFailed = results.Any(r => r.Domain is { IsSuccess: false });
            _cache.Set(key, results, anyFailed ? PartialFailureDuration : SuccessDuration);

            return results.ToList();
        }
    }
}
