using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class TldProviderRow
    {
        public ProviderEntry Provider { get; init; } = new();
        public PricedOffer? Registration { get; init; }
        public PricedOffer? Renewal { get; init; }
        public bool IsCheapest { get; init; }
    }
}
