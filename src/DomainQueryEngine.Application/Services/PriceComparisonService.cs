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

        /// <param name="period">
        /// Registration period in years to compare. Null uses each provider's shortest period; with a value,
        /// providers that do not offer that period get no price.
        /// </param>
        public async Task<PriceComparison> CompareAsync(IReadOnlyList<SearchResult> results, string? currency, int? period = null, CancellationToken ct = default)
        {
            var target = NormalizeCurrency(currency);
            var rates = await _exchangeRates.GetRatesAsync(ct);

            var items = results
                .Where(r => r.Domain != null)
                .Select(r => Compare(r, target, rates, period))
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

        private static ComparedResult Compare(SearchResult result, string target, ExchangeRates? rates, int? requestedPeriod)
        {
            var (period, original, regular) = ShownPrice(result.Domain, requestedPeriod);

            return new ComparedResult
            {
                Result = result,
                Period = period,
                OriginalPrice = original,
                DisplayPrice = Convert(original, target, rates),
                RegularPrice = Convert(regular, target, rates)
            };
        }

        private static BasePrice? Convert(BasePrice? price, string target, ExchangeRates? rates)
            => price != null && rates != null && rates.TryConvert(price.Amount, price.Currency, target, out var converted)
                ? new BasePrice { Amount = converted, Currency = target }
                : price;

        /// <summary>
        /// The price shown on a card: the registration price when the domain is available (for the requested
        /// period, or the shortest one), otherwise the shortest-period transfer price. Zero amounts mean "no price".
        /// </summary>
        private static (int? Period, BasePrice? Price, BasePrice? Regular) ShownPrice(DomainResult domain, int? requestedPeriod)
        {
            var prices = (domain.Prices ?? new List<DomainPrice>()).OrderBy(p => p.Period);

            if (domain.IsAvailable)
            {
                var entry = prices.FirstOrDefault(p => p.RegistrationPrice is { Amount: > 0 }
                                                       && (requestedPeriod == null || p.Period == requestedPeriod));
                var regular = entry?.RegularRegistrationPrice is { Amount: > 0 } r && r.Amount > entry.RegistrationPrice.Amount ? r : null;
                return (entry?.Period ?? requestedPeriod, entry?.RegistrationPrice, regular);
            }

            var transfer = prices.FirstOrDefault(p => p.TransferPrice is { Amount: > 0 });
            return (transfer?.Period, transfer?.TransferPrice, null);
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
