using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.Application.Catalog
{
    public enum TldSort
    {
        /// <summary>Popular extensions first, then the catalog's order.</summary>
        Popularity,
        CheapestRegistration,
        CheapestRenewal,
        Alphabetical
    }

    /// <summary>A provider's price, as quoted and in the display currency.</summary>
    public sealed class PricedOffer
    {
        public ProviderEntry Provider { get; init; } = new();
        public BasePrice Original { get; init; } = new();

        /// <summary><see cref="Original"/> in the display currency; the original itself when it could not be converted.</summary>
        public BasePrice Display { get; init; } = new();

        /// <summary>Regular price in the display currency when <see cref="Display"/> is a campaign price.</summary>
        public BasePrice? Regular { get; init; }

        public bool IsConverted => !string.Equals(Original.Currency, Display.Currency, StringComparison.OrdinalIgnoreCase);
        public bool IsCampaign => Regular != null && Regular.Amount > Display.Amount;
    }

    public sealed class TldListItem
    {
        public TldEntry Entry { get; init; } = new();
        public PricedOffer? CheapestRegistration { get; init; }
        public PricedOffer? CheapestRenewal { get; init; }
        public PricedOffer? CheapestTransfer { get; init; }
        public int ProviderCount { get; init; }
    }

    public sealed class TldPriceList
    {
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public bool IsSample { get; init; }
        public DateTime AsOf { get; init; }

        /// <summary>Number of extensions before the category filter.</summary>
        public int TotalCount { get; init; }

        /// <summary>Count per category, for filter tabs.</summary>
        public IReadOnlyDictionary<TldCategory, int> CategoryCounts { get; init; } = new Dictionary<TldCategory, int>();
        public List<TldListItem> Items { get; init; } = new();
    }

    public sealed class TldProviderRow
    {
        public ProviderEntry Provider { get; init; } = new();
        public PricedOffer? Registration { get; init; }
        public PricedOffer? Renewal { get; init; }
        public PricedOffer? Transfer { get; init; }
        public bool IsCheapest { get; init; }
    }

    public sealed class TldPriceDetail
    {
        public TldEntry Entry { get; init; } = new();
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public bool IsSample { get; init; }
        public DateTime AsOf { get; init; }
        public PricedOffer? CheapestRegistration { get; init; }

        /// <summary>Median 1-year registration price in the display currency (comparable prices only).</summary>
        public BasePrice? MedianRegistration { get; init; }

        /// <summary>Providers ordered by registration price in the display currency.</summary>
        public List<TldProviderRow> Rows { get; init; } = new();
    }
}
