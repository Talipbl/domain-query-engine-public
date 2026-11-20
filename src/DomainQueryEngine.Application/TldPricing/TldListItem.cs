using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class TldListItem
    {
        public TldEntry Entry { get; init; } = new();
        public PricedOffer? CheapestRegistration { get; init; }
        public PricedOffer? CheapestRenewal { get; init; }
        public int ProviderCount { get; init; }
    }
}
