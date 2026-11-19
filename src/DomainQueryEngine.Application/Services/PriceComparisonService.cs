using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;

namespace DomainQueryEngine.Application.Lookup
{
    /// <summary>
    /// Converts each result's price into the visitor's currency and sorts by it, so providers that
    /// quote in USD and TRY can be compared directly.
    /// </summary>
    public class PriceComparisonService
    {
        private readonly IExchangeRateProvider _exchangeRates;

        public PriceComparisonService(IExchangeRateProvider exchangeRates)
        {
            _exchangeRates = exchangeRates;
        }

        /// <summary>Returns the supported currency matching <paramref name="currency"/>, or TRY.</summary>
        public static string NormalizeCurrency(string? currency)
            => PriceComparison.SupportedCurrencies.FirstOrDefault(c => string.Equals(c, currency?.Trim(), StringComparison.OrdinalIgnoreCase))
               ?? ExchangeRates.BaseCurrency;

        public async Task<PriceComparison> CompareAsync(IReadOnlyList<SearchResult> results, string? currency, CancellationToken ct = default)
        {
            var target = NormalizeCurrency(currency);
            var rates = await _exchangeRates.GetRatesAsync(ct);

            var items = results
                .Where(r => r.Domain != null)
                .Select(r => Compare(r, target, rates))
                .ToList();

            // Without rates, keep the incoming order (local currency first, then grouped by currency).
            if (rates != null)
            {
                items = items
                    .OrderByDescending(i => i.Result.Domain.IsAvailable)
                    .ThenBy(i => ComparableAmount(i, target) == null)
                    .ThenBy(i => ComparableAmount(i, target) ?? decimal.MaxValue)
                    .ToList();
            }

            return new PriceComparison { Currency = target, Rates = rates, Items = items };
        }

        private static ComparedResult Compare(SearchResult result, string target, ExchangeRates? rates)
        {
            var (period, original) = ShownPrice(result.Domain);

            var display = original;
            if (original != null && rates != null
                && rates.TryConvert(original.Amount, original.Currency, target, out var converted))
            {
                display = new BasePrice { Amount = converted, Currency = target };
            }

            return new ComparedResult { Result = result, Period = period, OriginalPrice = original, DisplayPrice = display };
        }

        /// <summary>
        /// The price shown on a card: the shortest-period registration price when the domain is available,
        /// otherwise the shortest-period transfer price. Zero amounts mean "no price".
        /// </summary>
        private static (int? Period, BasePrice? Price) ShownPrice(DomainResult domain)
        {
            var prices = (domain.Prices ?? new List<DomainPrice>()).OrderBy(p => p.Period);
            var entry = domain.IsAvailable
                ? prices.FirstOrDefault(p => p.RegistrationPrice is { Amount: > 0 })
                : prices.FirstOrDefault(p => p.TransferPrice is { Amount: > 0 });

            var price = domain.IsAvailable ? entry?.RegistrationPrice : entry?.TransferPrice;
            return (entry?.Period, price);
        }

        /// <summary>Amount to sort by: only registration prices that are in the display currency.</summary>
        private static decimal? ComparableAmount(ComparedResult item, string target)
            => item.Result.Domain.IsAvailable
               && item.DisplayPrice != null
               && string.Equals(item.DisplayPrice.Currency, target, StringComparison.OrdinalIgnoreCase)
                ? item.DisplayPrice.Amount
                : null;
    }
}
