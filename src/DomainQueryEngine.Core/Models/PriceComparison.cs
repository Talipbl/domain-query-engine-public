namespace DomainQueryEngine.Core
{
    /// <summary>
    /// Search results arranged for one display currency.
    /// </summary>
    public sealed class PriceComparison
    {
        /// <summary>Currencies the visitor can choose from.</summary>
        public static readonly IReadOnlyList<string> SupportedCurrencies = new[] { "TRY", "USD", "EUR" };

        public string Currency { get; init; } = ExchangeRates.BaseCurrency;

        /// <summary>The rates used, or null when none were available and prices are shown as the providers sent them.</summary>
        public ExchangeRates? Rates { get; init; }

        public List<ComparedResult> Items { get; init; } = new();
    }

    public sealed class ComparedResult
    {
        public SearchResult Result { get; init; } = new();

        /// <summary>Period of the shown price, in years.</summary>
        public int? Period { get; init; }

        /// <summary>The price as the provider sent it: registration when available, transfer when only transferable.</summary>
        public BasePrice? OriginalPrice { get; init; }

        /// <summary><see cref="OriginalPrice"/> in the display currency; equal to it when no conversion was needed or possible.</summary>
        public BasePrice? DisplayPrice { get; init; }

        /// <summary>Regular price in the display currency when <see cref="DisplayPrice"/> is a campaign price.</summary>
        public BasePrice? RegularPrice { get; init; }

        public bool IsCampaign => RegularPrice != null && DisplayPrice != null && RegularPrice.Amount > DisplayPrice.Amount;

        public bool IsConverted => OriginalPrice != null && DisplayPrice != null
            && !string.Equals(OriginalPrice.Currency, DisplayPrice.Currency, StringComparison.OrdinalIgnoreCase);
    }
}
