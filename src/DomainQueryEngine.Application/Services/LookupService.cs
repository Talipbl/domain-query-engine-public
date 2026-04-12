using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using DomainQueryEngine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DomainQueryEngine.Application.Lookup
{

    public class LookupService : ILookupService
    {
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
                try
                {
                    var result = await strategy.LookupAsync(domain, ct);
                    var providerInfo = strategy.Provider;
                    providerInfo.VisitLink = string.Format(providerInfo.VisitLink, domain);
                    return new SearchResult
                    {
                        ProviderInfo = strategy.Provider,
                        Domain = result
                    };
                }
                catch (Exception ex)
                {
                    return new SearchResult
                    {
                        ProviderInfo = strategy.Provider,
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
            return ShortResultsByPrice(results.ToList());
        }

        private List<SearchResult> ShortResultsByPrice(List<SearchResult> results)
        {
            return results
                    .Where(p => p.Domain != null)
                    .OrderByDescending(p => p.Domain.IsAvailable)
                    .ThenBy(p =>
                    {
                        var lowest = p.Domain.Prices
                            .OrderBy(x => x.Period)
                            .FirstOrDefault();

                        return lowest?.RegistrationPrice?.Currency ?? "";
                    })
                    .ThenBy(p =>
                    {
                        var lowest = p.Domain.Prices
                            .OrderBy(x => x.Period)
                            .FirstOrDefault();

                        return lowest?.RegistrationPrice?.Amount ?? 0;
                    }).ToList();
        }
    }
}
