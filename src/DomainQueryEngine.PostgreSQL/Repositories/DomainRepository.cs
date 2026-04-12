using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainRepository : RepositoryBase<Domain>, IDomainRepository
    {
        public DomainRepository(AppDbContext context) : base(context) { }

        public async Task<Domain?> GetByNameAsync(string domainName)
            => await _dbSet.FirstOrDefaultAsync(d => d.DomainName == domainName);
    }
}
