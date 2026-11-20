using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Core.Abstractions
{
    public interface ISearchStatistics
    {
        Task<IReadOnlyList<PopularQuery>> GetMostSearchedAsync(TimeSpan window, int take, CancellationToken ct = default);
        Task<IReadOnlyList<PopularTld>> GetMostSearchedTldsAsync(TimeSpan window, int take, CancellationToken ct = default);
    }
}
