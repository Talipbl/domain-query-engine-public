using DomainQueryEngine.Application;
using DomainQueryEngine.Application.DomainSearch;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.SearchUI.Extensions;
using DomainQueryEngine.SearchUI.Helpers;

namespace DomainQueryEngine.SearchUI.Services
{
    /// <summary>
    /// Runs a domain search for the search page and the lookup API: asks the providers (through the
    /// cache), records the search in the history and prices the results in the visitor's currency.
    /// </summary>
    public sealed class DomainSearchService
    {
        private readonly ILookupService _lookupService;
        private readonly IDomainService _domainService;
        private readonly IProviderService _providerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PriceComparisonService _priceComparison;

        public DomainSearchService(ILookupService lookupService, IDomainService domainService, IProviderService providerService,
            IHttpContextAccessor httpContextAccessor, PriceComparisonService priceComparison)
        {
            _lookupService = lookupService;
            _domainService = domainService;
            _providerService = providerService;
            _httpContextAccessor = httpContextAccessor;
            _priceComparison = priceComparison;
        }

        /// <summary>Normalizes user input ("https://www.Ornek.com/a") to a registrable domain ("ornek.com").</summary>
        public static bool TryNormalize(string? input, out string domain)
        {
            domain = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                domain = DomainHelper.ValidateAndNormalizeDomain(input);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <param name="domain">An already normalized domain.</param>
        /// <param name="record">
        /// False when the page only re-shows a search (currency or filter change, back navigation); such
        /// requests are not counted as new searches.
        /// </param>
        public async Task<PriceComparison> SearchAsync(string domain, bool record, string? currency, int? period = null, CancellationToken ct = default)
        {
            var results = await _lookupService.LookupDomainAsync(domain, ct);

            if (record)
            {
                var search = await SaveSearchAsync(domain);
                await SaveResultsAsync(search, results);
            }

            // The history keeps the providers' original prices; conversion only affects what is shown.
            var context = _httpContextAccessor.HttpContext!;
            return await _priceComparison.CompareAsync(results, currency ?? CurrencyPreference.Get(context), period, ct);
        }

        private async Task<Domain> SaveSearchAsync(string domain)
        {
            var context = _httpContextAccessor.HttpContext!;
            return await _domainService.AddDomainSearch(new Domain
            {
                DomainName = domain,
                Tld = DomainHelper.GetTldFromDomainName(domain),
                CreatedAt = DateTime.UtcNow,
                UserIp = NetworkHelper.GetRequestIp(_httpContextAccessor),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                SessionId = _httpContextAccessor.GetOrSetCookie()
            });
        }

        private async Task SaveResultsAsync(Domain search, List<SearchResult> results)
        {
            foreach (var result in results)
            {
                var provider = await _providerService.GetOrCreateProviderAsync(result.ProviderInfo);
                var shortest = result.Domain?.Prices?.OrderBy(p => p.Period).FirstOrDefault();

                await _lookupService.AddDomainLookupResult(new DomainLookupResult
                {
                    DomainId = search.Id,
                    ProviderId = provider.Id,
                    IsSuccess = result.Domain?.IsSuccess ?? false,
                    IsAvailable = result.Domain?.IsAvailable ?? false,
                    ResponseTimeMs = result.ResponseTimeMs,
                    RawResponse = result.Domain?.RawResponse ?? string.Empty,
                    PriceAmount = shortest?.RegistrationPrice?.Amount,
                    PriceCurrency = shortest?.RegistrationPrice?.Currency,
                    PricePeriod = shortest?.Period,
                    CheckedAt = DateTime.UtcNow
                });
            }
        }
    }
}
