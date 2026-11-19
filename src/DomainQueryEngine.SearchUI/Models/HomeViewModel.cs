using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class HomeViewModel
    {
        /// <summary>Popular extensions with their cheapest offers, for the cards under the search box.</summary>
        public List<TldListItem> PopularTlds { get; init; } = new();

        /// <summary>The list the cards come from (currency, rates, sample flag).</summary>
        public TldPriceList TldList { get; init; } = new();

        /// <summary>Most searched extensions in the last 24 hours.</summary>
        public List<MostSearchedTld> MostSearchedTlds { get; init; } = new();
        public bool MostSearchedIsSample { get; init; }

        /// <summary>Providers whose prices are compared, in Turkish alphabetical order.</summary>
        public List<ProviderEntry> ActiveProviders { get; init; } = new();

        public bool AnyConverted => PopularTlds.Any(t => t.CheapestRegistration?.IsConverted == true);
    }

    /// <param name="HasPricePage">The catalog has a price page for it (/uzantilar/{slug}).</param>
    public sealed record MostSearchedTld(PopularTld Tld, bool HasPricePage);
}
