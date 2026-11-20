using DomainQueryEngine.Core;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class TldPriceDetail
    {
        public TldEntry Entry { get; init; } = new();
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public DateTime AsOf { get; init; }
        public PricedOffer? CheapestRegistration { get; init; }
        public List<TldProviderRow> Rows { get; init; } = new();
    }
}
