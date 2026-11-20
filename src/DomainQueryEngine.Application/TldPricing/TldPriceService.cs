using DomainQueryEngine.Application.DomainSearch;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    /// <summary>
    /// Prepares the extension price pages: converts catalog prices to the visitor's currency and picks
    /// the cheapest offers, with the same rules as the search results (campaign price when offered,
    /// TCMB rates, prices that cannot be converted are not compared).
    /// </summary>
    public class TldPriceService
    {
        private readonly ITldPriceCatalog _tlds;
        private readonly IProviderDirectory _providers;
        private readonly IExchangeRateProvider _exchangeRates;

        public TldPriceService(ITldPriceCatalog tlds, IProviderDirectory providers, IExchangeRateProvider exchangeRates)
        {
            _tlds = tlds;
            _providers = providers;
            _exchangeRates = exchangeRates;
        }

        public async Task<TldPriceList> GetListAsync(string? currency, TldSort sort = TldSort.Alphabetical, CancellationToken ct = default)
        {
            var target = PriceComparisonService.NormalizeCurrency(currency);
            var rates = await _exchangeRates.GetRatesAsync(ct);
            var providers = await LoadProvidersAsync(ct);
            var entries = await _tlds.GetAllAsync(ct);

            var items = entries
                .Select(e => new TldListItem
                {
                    Entry = e,
                    CheapestRegistration = Cheapest(e, p => p.Registration, p => p.RegistrationTermYears, target, rates, providers, withRegular: true),
                    CheapestRenewal = Cheapest(e, p => p.Renewal, p => p.RenewalTermYears, target, rates, providers),
                    ProviderCount = e.Prices.Count(p => p.Registration is > 0 || p.Renewal is > 0)
                });

            items = sort switch
            {
                TldSort.CheapestRegistration => items.OrderBy(i => SortKey(i.CheapestRegistration, target)).ThenBy(i => i.Entry.Tld),
                TldSort.CheapestRenewal => items.OrderBy(i => SortKey(i.CheapestRenewal, target)).ThenBy(i => i.Entry.Tld),
                TldSort.Alphabetical => items.OrderBy(i => i.Entry.Tld, StringComparer.Ordinal),
                _ => items.OrderBy(i => i.Entry.Tld, StringComparer.Ordinal)
            };

            return new TldPriceList
            {
                Currency = target,
                Rates = rates,
                AsOf = await _tlds.GetAsOfAsync(ct),
                Items = items.ToList()
            };
        }

        /// <returns>Null when the catalog does not know the extension.</returns>
        public async Task<TldPriceDetail?> GetDetailAsync(string tld, string? currency, CancellationToken ct = default)
        {
            var entry = await _tlds.GetAsync(tld, ct);
            if (entry == null)
                return null;

            var target = PriceComparisonService.NormalizeCurrency(currency);
            var rates = await _exchangeRates.GetRatesAsync(ct);
            var providers = await LoadProvidersAsync(ct);

            var rows = entry.Prices
                .Select(p => new TldProviderRow
                {
                    Provider = ProviderFor(p.Provider, providers),
                    Registration = Offer(p, p.Registration, p.RegistrationTermYears, target, rates, providers, withRegular: true),
                    Renewal = Offer(p, p.Renewal, p.RenewalTermYears, target, rates, providers)
                })
                .OrderBy(r => SortKey(r.Registration, target))
                .ThenBy(r => r.Provider.Name, StringComparer.CurrentCulture)
                .ToList();

            var cheapest = rows.FirstOrDefault(r => IsComparable(r.Registration, target))?.Registration;
            rows = rows.Select(r => new TldProviderRow
            {
                Provider = r.Provider,
                Registration = r.Registration,
                Renewal = r.Renewal,
                IsCheapest = cheapest != null && ReferenceEquals(r.Registration, cheapest)
            }).ToList();

            return new TldPriceDetail
            {
                Entry = entry,
                Currency = target,
                Rates = rates,
                AsOf = await _tlds.GetAsOfAsync(ct),
                CheapestRegistration = cheapest,
                Rows = rows
            };
        }

        private async Task<Dictionary<string, ProviderEntry>> LoadProvidersAsync(CancellationToken ct)
            => (await _providers.GetAllAsync(ct)).ToDictionary(p => p.Slug, StringComparer.OrdinalIgnoreCase);

        private static ProviderEntry ProviderFor(string slug, IReadOnlyDictionary<string, ProviderEntry> providers)
            => providers.TryGetValue(slug, out var provider) ? provider : new ProviderEntry { Slug = slug, Name = slug };

        private static PricedOffer? Cheapest(TldEntry entry, Func<TldProviderPrice, decimal?> select, Func<TldProviderPrice, int> term, string target,
            ExchangeRates? rates, IReadOnlyDictionary<string, ProviderEntry> providers, bool withRegular = false)
            => entry.Prices
                .Select(p => Offer(p, select(p), term(p), target, rates, providers, withRegular))
                .Where(o => o != null)
                .OrderBy(o => SortKey(o, target))
                .FirstOrDefault();

        private static PricedOffer? Offer(TldProviderPrice price, decimal? amount, int termYears, string target, ExchangeRates? rates,
            IReadOnlyDictionary<string, ProviderEntry> providers, bool withRegular = false)
        {
            if (amount is not > 0)
                return null;

            var original = new BasePrice { Amount = amount.Value, Currency = price.Currency };
            var regular = withRegular && price.IsCampaign
                ? Convert(new BasePrice { Amount = price.RegularRegistration!.Value, Currency = price.Currency }, target, rates)
                : null;

            return new PricedOffer
            {
                Provider = ProviderFor(price.Provider, providers),
                Original = original,
                Display = Convert(original, target, rates),
                Regular = regular,
                TermYears = termYears
            };
        }

        private static BasePrice Convert(BasePrice price, string target, ExchangeRates? rates)
            => rates != null && rates.TryConvert(price.Amount, price.Currency, target, out var converted)
                ? new BasePrice { Amount = converted, Currency = target }
                : price;

        private static bool IsComparable(PricedOffer? offer, string target)
            => offer != null && string.Equals(offer.Display.Currency, target, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Prices in the display currency sort by amount; prices that could not be converted come after them
        /// (TRY first), and missing prices last.
        /// </summary>
        private static (int Group, string Currency, decimal Amount) SortKey(PricedOffer? offer, string target)
        {
            if (offer == null) return (3, string.Empty, decimal.MaxValue);
            if (IsComparable(offer, target)) return (0, string.Empty, offer.Display.Amount);
            var isLira = string.Equals(offer.Display.Currency, ExchangeRates.BaseCurrency, StringComparison.OrdinalIgnoreCase);
            return (isLira ? 1 : 2, offer.Display.Currency, offer.Display.Amount);
        }

    }
}
