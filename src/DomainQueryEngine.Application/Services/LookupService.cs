using System.Diagnostics;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using DomainQueryEngine.Core.Utilities;

namespace DomainQueryEngine.Application.Lookup
{
    public class LookupService : ILookupService
    {
        /// <summary>
        /// Prices in the local currency are listed before prices in other currencies, because
        /// amounts in different currencies cannot be compared without exchange rates.
        /// </summary>
        private const string PreferredCurrency = "TRY";

        private readonly IEnumerable<IDomainLookupStrategy> _strategies;
        private readonly IDomainLookupResultRepository _lookupResultRepository;

        public LookupService(IEnumerable<IDomainLookupStrategy> strategies, IDomainLookupResultRepository lookupResultRepository)
        {
            _strategies = strategies;
            _lookupResultRepository = lookupResultRepository;
        }

        public async Task AddDomainLookupResult(DomainLookupResult lookupResult)
        {
            await _lookupResultRepository.AddAsync(lookupResult);
        }

        public async Task<List<SearchResult>> LookupDomainAsync(string domain, CancellationToken ct = default)
        {
            var tasks = _strategies.Select(async strategy =>
            {
                var visitLink = BuildVisitLink(strategy.Provider.VisitLink, domain);
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var result = await strategy.LookupAsync(domain, ct);
                    return new SearchResult
                    {
                        ProviderInfo = strategy.Provider,
                        Domain = result,
                        VisitLink = visitLink,
                        ResponseTimeMs = stopwatch.ElapsedMilliseconds
                    };
                }
                catch (Exception ex)
                {
                    return new SearchResult
                    {
                        ProviderInfo = strategy.Provider,
                        VisitLink = visitLink,
                        ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                        Domain = new DomainResult
                        {
                            DomainName = domain,
                            Tld = DomainHelper.GetTldFromDomainName(domain),
                            IsSuccess = false,
                            IsValidDomain = false,
                            RawResponse = $"Error: {ex.Message}",
                            CheckedAt = DateTime.UtcNow
                        }
                    };
                }
            });

            var results = await Task.WhenAll(tasks);
            return SortResults(results);
        }

        /// <summary>
        /// Fills the domain into the provider's link template without changing the template itself.
        /// </summary>
        private static string BuildVisitLink(string? template, string domain)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            return template.Contains("{0}")
                ? string.Format(template, Uri.EscapeDataString(domain))
                : template;
        }

        /// <summary>
        /// Sort order: available first, then results that have a price, then the preferred currency,
        /// then by currency, then by the shortest-period registration price (cheapest first).
        /// </summary>
        internal static List<SearchResult> SortResults(IEnumerable<SearchResult> results)
        {
            return results
                .Where(r => r.Domain != null)
                .Select(r => new { Result = r, Price = GetShortestPeriodPrice(r.Domain) })
                .OrderByDescending(x => x.Result.Domain.IsAvailable)
                .ThenBy(x => x.Price == null)
                .ThenBy(x => string.Equals(x.Price?.Currency, PreferredCurrency, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(x => x.Price?.Currency ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Price?.Amount ?? decimal.MaxValue)
                .Select(x => x.Result)
                .ToList();
        }

        private static BasePrice? GetShortestPeriodPrice(DomainResult domain)
        {
            var price = domain.Prices?
                .OrderBy(p => p.Period)
                .FirstOrDefault()?
                .RegistrationPrice;

            // Parsers use an empty BasePrice (Amount = 0) when a price could not be read.
            return price != null && price.Amount > 0 ? price : null;
        }
    }
}
