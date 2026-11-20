using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class ProviderDetailPageViewModel
    {
        public ProviderPriceList Prices { get; init; } = new();
        public ProviderEntry Provider => Prices.Provider;
        public bool AnyConverted => Prices.Rows.Any(r => r.Registration?.IsConverted == true || r.Renewal?.IsConverted == true);
    }
}
