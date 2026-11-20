using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class HomeViewModel
    {
        /// <summary>Popular extensions with their cheapest offers, for the cards under the search box.</summary>
        public List<TldListItem> PopularTlds { get; init; } = new();

        /// <summary>The list the cards come from (currency and rates).</summary>
        public TldPriceList TldList { get; init; } = new();

        /// <summary>Most searched extensions in the last 24 hours.</summary>
        public List<MostSearchedTld> MostSearchedTlds { get; init; } = new();

        /// <summary>Providers shown in the homepage logo carousel.</summary>
        public List<ProviderEntry> Providers { get; init; } = new();

        public bool AnyConverted => PopularTlds.Any(t => t.CheapestRegistration?.IsConverted == true || t.CheapestRenewal?.IsConverted == true);
    }

}
