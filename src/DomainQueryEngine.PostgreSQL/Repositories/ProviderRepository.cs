using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class ProviderRepository : RepositoryBase<Provider>, IProviderRepository
    {
        public ProviderRepository(AppDbContext context) : base(context) { }

        public async Task<Provider?> GetByNameAsync(string name)
            => await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
    }
}
