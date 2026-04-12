using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface IDomainRepository : IRepository<Domain>
    {
        Task<Domain?> GetByNameAsync(string domainName);
    }
}
