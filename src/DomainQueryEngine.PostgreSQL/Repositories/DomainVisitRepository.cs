using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainVisitRepository : RepositoryBase<DomainVisit>, IDomainVisitRepository
    {
        public DomainVisitRepository(AppDbContext context) : base(context) { }

        public async Task<List<DomainVisit>> GetByDomainIdAsync(Guid domainId)
            => await _dbSet.Where(v => v.DomainId == domainId).ToListAsync();

        public async Task<List<DomainVisit>> GetByProviderIdAsync(Guid providerId)
            => await _dbSet.Where(v => v.ProviderId == providerId).ToListAsync();
    }
}
