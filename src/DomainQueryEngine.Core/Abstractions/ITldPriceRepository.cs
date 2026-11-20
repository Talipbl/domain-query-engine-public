using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Core.Abstractions
{
    /// <summary>Reads and atomically replaces one provider's current TLD price list.</summary>
    public interface ITldPriceRepository
    {
        Task ReplaceProviderPricesAsync(TldPriceSnapshot snapshot, CancellationToken ct = default);
        Task RecordProviderPriceFailureAsync(string providerSlug, string sourceUrl, DateTime attemptedAt, string error, CancellationToken ct = default);
    }
}
