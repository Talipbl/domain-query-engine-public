using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Application.Lookup
{
    public interface ILookupService
    {
        Task<List<SearchResult>> LookupDomainAsync(string domain, CancellationToken ct = default);
        Task AddDomainLookupResult(DomainLookupResult lookupResult);
    }
}
