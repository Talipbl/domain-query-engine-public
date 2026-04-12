using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Application.Lookup
{
    public interface IProviderService
    {
        Task<Provider> GetOrCreateProviderAsync(ProviderInfo info);
    }
}
