using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Core.Abstractions
{
    /// <summary>Reads the current registration and renewal prices collected from providers.</summary>
    public interface ITldPriceCatalog
    {
        Task<DateTime> GetAsOfAsync(CancellationToken ct = default);
        Task<IReadOnlyList<TldEntry>> GetAllAsync(CancellationToken ct = default);
        Task<TldEntry?> GetAsync(string tld, CancellationToken ct = default);
    }
}
