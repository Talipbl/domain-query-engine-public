namespace DomainQueryEngine.Core.TldPricing
{
    /// <summary>A complete, validated price-list fetch from one provider.</summary>
    public sealed class TldPriceSnapshot
    {
        public string ProviderSlug { get; init; } = string.Empty;
        public string SourceUrl { get; init; } = string.Empty;
        public DateTime ObservedAt { get; init; } = DateTime.UtcNow;
        public IReadOnlyList<TldProviderPrice> Prices { get; init; } = Array.Empty<TldProviderPrice>();
    }
}
