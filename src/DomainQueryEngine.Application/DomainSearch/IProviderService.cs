using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Application.DomainSearch
{
    public interface IProviderService
    {
        Task<Provider> GetOrCreateProviderAsync(ProviderInfo info);
    }
}
