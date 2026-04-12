using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainLookupResultRepository : RepositoryBase<DomainLookupResult>, IDomainLookupResultRepository
    {
        public DomainLookupResultRepository(AppDbContext context) : base(context) { }

        public async Task<List<DomainLookupResult>> GetByDomainIdAsync(Guid domainId)
            => await _dbSet.Where(l => l.DomainId == domainId).ToListAsync();

        public async Task<List<DomainLookupResult>> GetByProviderIdAsync(Guid providerId)
            => await _dbSet.Where(l => l.ProviderId == providerId).ToListAsync();
    }
}
