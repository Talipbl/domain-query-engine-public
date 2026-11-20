using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    /// <param name="HasPricePage">The catalog has a price page for it (/uzantilar/{slug}).</param>
    public sealed record MostSearchedTld(PopularTld Tld, bool HasPricePage);
}
