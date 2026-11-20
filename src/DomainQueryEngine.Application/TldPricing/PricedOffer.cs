using DomainQueryEngine.Core;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.TldPricing
{
    /// <summary>A provider's current price, in its original and display currencies, for the minimum term.</summary>
    public sealed class PricedOffer
    {
        public ProviderEntry Provider { get; init; } = new();
        public BasePrice Original { get; init; } = new();
        public BasePrice Display { get; init; } = new();
        public BasePrice? Regular { get; init; }
        public int TermYears { get; init; } = 1;
        public bool IsConverted => !string.Equals(Original.Currency, Display.Currency, StringComparison.OrdinalIgnoreCase);
        public bool IsCampaign => Regular != null && Regular.Amount > Display.Amount;
    }
}
