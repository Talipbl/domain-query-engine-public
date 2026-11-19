using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.SearchUI.Models
{
    /// <summary>Model for Components/_DataNote.</summary>
    public sealed record DataNoteModel(bool IsSample, DateTime AsOf, ExchangeRates? Rates, bool AnyConverted);

    /// <summary>Model for Components/_Offer: a price cell.</summary>
    public sealed record OfferCellModel(PricedOffer? Offer, bool ShowProvider = false, bool ShowDetails = false, bool Highlight = false);

    /// <param name="LabelKey">Localization key of the tab label.</param>
    public sealed record TldGroup(string Key, string LabelKey, TldCategory? Category, bool PopularOnly = false);

    /// <param name="LabelKey">Localization key of the option label.</param>
    public sealed record TldSortOption(string Key, string LabelKey, TldSort Sort);

    public sealed class TldListPageViewModel
    {
        public static readonly IReadOnlyList<TldGroup> Groups = new TldGroup[]
        {
            new("tumu", "Tlds.Group.All", null),
            new("populer", "Tlds.Group.Popular", null, PopularOnly: true),
            new("tr", "Tlds.Group.Turkish", TldCategory.Turkish),
            new("genel", "Tlds.Group.Generic", TldCategory.Generic),
            new("yeni", "Tlds.Group.New", TldCategory.New),
            new("ulke", "Tlds.Group.Country", TldCategory.Country)
        };

        public static readonly IReadOnlyList<TldSortOption> Sorts = new TldSortOption[]
        {
            new("populerlik", "Tlds.Sort.Popularity", TldSort.Popularity),
            new("kayit", "Tlds.Sort.Registration", TldSort.CheapestRegistration),
            new("yenileme", "Tlds.Sort.Renewal", TldSort.CheapestRenewal),
            new("alfabe", "Tlds.Sort.Alphabetical", TldSort.Alphabetical)
        };

        public TldPriceList List { get; init; } = new();
        public List<TldListItem> Items { get; init; } = new();
        public TldGroup Group { get; init; } = Groups[0];
        public TldSortOption Sort { get; init; } = Sorts[0];
        public string Query { get; init; } = string.Empty;
        public IReadOnlyDictionary<string, int> GroupCounts { get; init; } = new Dictionary<string, int>();
        public bool AnyConverted => Items.Any(i => i.CheapestRegistration?.IsConverted == true || i.CheapestRenewal?.IsConverted == true || i.CheapestTransfer?.IsConverted == true);
    }

    public sealed class TldDetailPageViewModel
    {
        public TldDetailPageViewModel(TldPriceDetail detail)
        {
            Detail = detail;

            var max = detail.Entry.History.Count == 0 ? 0 : detail.Entry.History.Max(p => p.Amount);
            History = detail.Entry.History
                .Select((p, i) => new HistoryBar(p, max > 0 ? (int)Math.Round(p.Amount / max * 100) : 0, i == detail.Entry.History.Count - 1))
                .ToList();
        }

        public TldPriceDetail Detail { get; }
        public TldEntry Entry => Detail.Entry;
        public List<HistoryBar> History { get; }
        public bool AnyConverted => Detail.Rows.Any(r => r.Registration?.IsConverted == true || r.Renewal?.IsConverted == true || r.Transfer?.IsConverted == true);

        /// <summary>Registration prices above this are flagged as unusually high (5 × median).</summary>
        public bool IsOutlier(PricedOffer? offer)
            => offer != null && Detail.MedianRegistration is { Amount: > 0 } median
               && string.Equals(offer.Display.Currency, median.Currency, StringComparison.OrdinalIgnoreCase)
               && offer.Display.Amount > median.Amount * ResultsViewModel.OutlierFactor
               && Detail.Rows.Count(r => r.Registration != null) >= 3;
    }

    /// <summary>One month in the price history chart; <see cref="HeightPercent"/> is relative to the highest month.</summary>
    public sealed record HistoryBar(PricePoint Point, int HeightPercent, bool IsLatest)
    {
        /// <summary>
        /// CSS height of the bar: 80% of the column at most (room for the value label), at least 4%.
        /// An integer, so it never renders with a culture-specific decimal separator ("73,6%").
        /// </summary>
        public int BarHeightPercent => Math.Max(HeightPercent * 4 / 5, 4);

        /// <summary>Short month name in the request's language ("Kas", "Nov").</summary>
        public string Label => Point.Month.Length >= 7 && int.TryParse(Point.Month[5..7], out var m) && m is >= 1 and <= 12
            ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m)
            : Point.Month;
    }

    public sealed class ProviderListPageViewModel
    {
        public List<ProviderEntry> Providers { get; init; } = new();
        public bool IsSample { get; init; }
    }

    public sealed class ProviderDetailPageViewModel
    {
        public ProviderPriceList Prices { get; init; } = new();
        public ProviderEntry Provider => Prices.Provider;
        public bool AnyConverted => Prices.Rows.Any(r => r.Registration?.IsConverted == true || r.Renewal?.IsConverted == true || r.Transfer?.IsConverted == true);
    }
}
