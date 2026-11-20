using DomainQueryEngine.Core;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    public sealed class ProviderPriceList
    {
        public ProviderEntry Provider { get; init; } = new();
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public DateTime AsOf { get; init; }
        public List<ProviderTldRow> Rows { get; init; } = new();
    }
}
