using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface IDomainVisitRepository : IRepository<DomainVisit>
    {
        Task<List<DomainVisit>> GetByDomainIdAsync(Guid domainId);
        Task<List<DomainVisit>> GetByProviderIdAsync(Guid providerId);
    }
}
