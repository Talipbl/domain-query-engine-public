using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Task<Provider?> GetByNameAsync(string name);
    }
}
