using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Core.Abstractions
{
    /// <summary>Fetches and parses the current TLD prices published by one provider.</summary>
    public interface ITldPriceSource
    {
        string ProviderSlug { get; }
        string SourceUrl { get; }
        Task<TldPriceSnapshot> FetchAsync(CancellationToken ct = default);
    }
}
