using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.SearchUI.Models
{
    /// <summary>Model for Components/_DataNote.</summary>
    public sealed record DataNoteModel(bool IsSample, DateTime AsOf, ExchangeRates? Rates, bool AnyConverted);

    /// <summary>Model for Components/_Offer: a price cell.</summary>
    public sealed record OfferCellModel(PricedOffer? Offer, bool ShowProvider = false, bool ShowDetails = false, bool Highlight = false);

    public sealed record TldGroup(string Key, string Label, TldCategory? Category, bool PopularOnly = false);

    public sealed record TldSortOption(string Key, string Label, TldSort Sort);

    public sealed class TldListPageViewModel
    {
        public static readonly IReadOnlyList<TldGroup> Groups = new TldGroup[]
        {
            new("tumu", "Tümü", null),
            new("populer", "Popüler", null, PopularOnly: true),
            new("tr", ".tr uzantıları", TldCategory.Turkish),
            new("genel", "Genel", TldCategory.Generic),
            new("yeni", "Yeni uzantılar", TldCategory.New),
            new("ulke", "Ülke kodlu", TldCategory.Country)
        };

        public static readonly IReadOnlyList<TldSortOption> Sorts = new TldSortOption[]
        {
            new("populerlik", "Popülerlik", TldSort.Popularity),
            new("kayit", "En ucuz kayıt", TldSort.CheapestRegistration),
            new("yenileme", "En ucuz yenileme", TldSort.CheapestRenewal),
            new("alfabe", "Alfabetik", TldSort.Alphabetical)
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
        private static readonly string[] MonthNames = { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

        /// <summary>
        /// CSS height of the bar: 80% of the column at most (room for the value label), at least 4%.
        /// An integer, so it never renders with a culture-specific decimal separator ("73,6%").
        /// </summary>
        public int BarHeightPercent => Math.Max(HeightPercent * 4 / 5, 4);

        public string Label => Point.Month.Length >= 7 && int.TryParse(Point.Month[5..7], out var m) && m is >= 1 and <= 12
            ? MonthNames[m - 1]
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
