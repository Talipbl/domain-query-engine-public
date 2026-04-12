using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Application
{
    public interface IDomainService
    {
        Task<Domain> AddDomainSearch(Domain domain);
    }
}
