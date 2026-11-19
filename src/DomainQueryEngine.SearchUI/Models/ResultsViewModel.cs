using DomainQueryEngine.Core;

namespace DomainQueryEngine.SearchUI.Models
{
    /// <summary>
    /// What the results view shows, derived from a <see cref="PriceComparison"/> so the view holds no logic.
    /// </summary>
    public sealed class ResultsViewModel
    {
        /// <summary>A price this many times the median of the other offers is flagged as unusually high.</summary>
        public const decimal OutlierFactor = 5m;

        public string DomainName { get; init; } = string.Empty;
        public string Currency { get; init; } = ExchangeRates.BaseCurrency;
        public ExchangeRates? Rates { get; init; }
        public int AskedCount { get; init; }
        public int AvailableCount { get; init; }
        public ResultRowViewModel? Best { get; init; }
        public List<ResultRowViewModel> Rows { get; init; } = new();
        public List<FailedProvider> Failed { get; init; } = new();
        public bool AnyConverted => Rows.Any(r => r.Item.IsConverted);

        public static ResultsViewModel From(PriceComparison comparison)
        {
            var succeeded = comparison.Items.Where(i => i.Result.Domain.IsSuccess).ToList();
            var failed = comparison.Items.Where(i => !i.Result.Domain.IsSuccess).ToList();

            var availablePrices = succeeded
                .Where(i => i.Result.Domain.IsAvailable && i.DisplayPrice != null)
                .Select(i => i.DisplayPrice!.Amount)
                .OrderBy(a => a)
                .ToList();
            var median = availablePrices.Count > 0 ? availablePrices[availablePrices.Count / 2] : (decimal?)null;

            // Items arrive sorted (cheapest available first), so the first priced available one is the best.
            var bestItem = succeeded.FirstOrDefault(i => i.Result.Domain.IsAvailable && i.DisplayPrice != null);

            var rows = succeeded.Select(item => new ResultRowViewModel
            {
                Item = item,
                IsBest = ReferenceEquals(item, bestItem),
                Warning = availablePrices.Count >= 3 && median is > 0 && item.DisplayPrice != null
                          && item.Result.Domain.IsAvailable && item.DisplayPrice.Amount > median.Value * OutlierFactor
                    ? "Diğer sağlayıcılardan çok yüksek"
                    : null
            }).ToList();

            return new ResultsViewModel
            {
                DomainName = comparison.Items.Select(i => i.Result.Domain.DomainName).FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? string.Empty,
                Currency = comparison.Currency,
                Rates = comparison.Rates,
                AskedCount = comparison.Items.Count,
                AvailableCount = succeeded.Count(i => i.Result.Domain.IsAvailable),
                Best = rows.FirstOrDefault(r => r.IsBest),
                Rows = rows,
                Failed = failed.Select(i => new FailedProvider(i.Result.ProviderInfo?.Name ?? "Sağlayıcı", DescribeError(i.Result.Domain.RawResponse))).ToList()
            };
        }

        /// <summary>Turns the stored error ("Error: …") into a short, readable reason.</summary>
        public static string DescribeError(string? raw)
        {
            raw ??= string.Empty;
            if (raw.Contains("Timed out", StringComparison.OrdinalIgnoreCase)) return "Zamanında yanıt vermedi";
            if (raw.Contains("404")) return "Sorgu adresi bulunamadı (404)";
            if (raw.Contains("403")) return "Sağlayıcı erişimi engelledi (403)";
            if (raw.Contains("E_KEY_ERROR")) return "Sağlayıcı doğrulama istedi";
            if (System.Text.RegularExpressions.Regex.IsMatch(raw, @"\b5\d\d\b")) return "Sağlayıcıda sunucu hatası";
            return "Yanıt alınamadı";
        }
    }

    public sealed class ResultRowViewModel
    {
        public ComparedResult Item { get; init; } = new();
        public bool IsBest { get; init; }
        public string? Warning { get; init; }
    }

    public sealed record FailedProvider(string Name, string Reason);
}
