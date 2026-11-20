using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface IProviderDirectory
    {
        Task<IReadOnlyList<ProviderEntry>> GetAllAsync(CancellationToken ct = default);
        Task<ProviderEntry?> GetAsync(string slug, CancellationToken ct = default);
    }
}
