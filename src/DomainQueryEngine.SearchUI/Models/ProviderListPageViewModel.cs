using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class ProviderListPageViewModel
    {
        public List<ProviderEntry> Providers { get; init; } = new();
    }
}
