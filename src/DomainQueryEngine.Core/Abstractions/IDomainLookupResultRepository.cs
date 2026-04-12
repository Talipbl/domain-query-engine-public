using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface IDomainLookupResultRepository : IRepository<DomainLookupResult>
    {
        Task<List<DomainLookupResult>> GetByDomainIdAsync(Guid domainId);
        Task<List<DomainLookupResult>> GetByProviderIdAsync(Guid providerId);
    }
}
