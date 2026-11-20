using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class ProviderTldRow
    {
        public TldEntry Entry { get; init; } = new();
        public PricedOffer? Registration { get; init; }
        public PricedOffer? Renewal { get; init; }
    }
}
