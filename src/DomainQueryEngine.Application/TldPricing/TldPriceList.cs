using DomainQueryEngine.Core;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class TldPriceList
    {
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public DateTime AsOf { get; init; }
        public List<TldListItem> Items { get; init; } = new();
    }
}
