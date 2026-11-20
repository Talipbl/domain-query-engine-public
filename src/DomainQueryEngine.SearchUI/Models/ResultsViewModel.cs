using DomainQueryEngine.Core;

namespace DomainQueryEngine.SearchUI.Models
{
    /// <summary>What the results view shows, derived from a price comparison.</summary>
    public sealed class ResultsViewModel
    {
        public const decimal OutlierFactor = 5m;
        public string DomainName { get; init; } = string.Empty;
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public SearchFilters Filters { get; init; } = new();
        public int AskedCount { get; init; }
        public int RespondedCount { get; init; }
        public int AvailableCount { get; init; }
        public ResultRowViewModel? Best { get; init; }
        public List<ResultRowViewModel> Rows { get; init; } = new();
        public List<FailedProvider> Failed { get; init; } = new();
        public bool AnyConverted => Rows.Any(r => r.Item.IsConverted);

        public static ResultsViewModel From(PriceComparison comparison, SearchFilters? filters = null)
        {
            filters ??= new SearchFilters();
            var succeeded = comparison.Items.Where(i => i.Result.Domain.IsSuccess).ToList();
            var failed = comparison.Items.Where(i => !i.Result.Domain.IsSuccess).ToList();
            var availablePrices = succeeded.Where(i => i.Result.Domain.IsAvailable && i.DisplayPrice != null)
                .Select(i => i.DisplayPrice!.Amount).OrderBy(a => a).ToList();
            var median = availablePrices.Count > 0 ? availablePrices[availablePrices.Count / 2] : (decimal?)null;
            var bestItem = succeeded.FirstOrDefault(i => i.Result.Domain.IsAvailable && i.DisplayPrice != null);

            var rows = succeeded.Select(item => new ResultRowViewModel
            {
                Item = item,
                IsBest = ReferenceEquals(item, bestItem),
                IsOutlier = availablePrices.Count >= 3 && median is > 0 && item.DisplayPrice != null
                    && item.Result.Domain.IsAvailable && item.DisplayPrice.Amount > median.Value * OutlierFactor
            });

            if (filters.OnlyAvailable)
                rows = rows.Where(r => r.Item.Result.Domain.IsAvailable);
            if (filters.Sort == ResultSort.Name)
                rows = rows.OrderBy(r => r.Item.Result.ProviderInfo?.Name,
                    StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true));

            var rowList = rows.ToList();
            return new ResultsViewModel
            {
                DomainName = comparison.Items.Select(i => i.Result.Domain.DomainName).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? string.Empty,
                Currency = comparison.Currency,
                Rates = comparison.Rates,
                Filters = filters,
                AskedCount = comparison.Items.Count,
                RespondedCount = succeeded.Count,
                AvailableCount = succeeded.Count(i => i.Result.Domain.IsAvailable),
                Best = rowList.FirstOrDefault(r => r.IsBest),
                Rows = rowList,
                Failed = failed.Select(i => new FailedProvider(i.Result.ProviderInfo?.Name ?? string.Empty, ErrorKey(i.Result.Domain.RawResponse))).ToList()
            };
        }

        public static string ErrorKey(string? raw)
        {
            raw ??= string.Empty;
            if (raw.Contains("Timed out", StringComparison.OrdinalIgnoreCase)) return "Error.Timeout";
            if (raw.Contains("404")) return "Error.NotFound";
            if (raw.Contains("403")) return "Error.Forbidden";
            if (raw.Contains("E_KEY_ERROR")) return "Error.Challenge";
            if (System.Text.RegularExpressions.Regex.IsMatch(raw, @"\b5\d\d\b")) return "Error.Server";
            return "Error.Unknown";
        }
    }
}
