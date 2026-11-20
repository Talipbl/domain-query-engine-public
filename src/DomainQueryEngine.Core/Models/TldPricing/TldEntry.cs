namespace DomainQueryEngine.Core.TldPricing
{
    /// <summary>An extension represented by the provider prices currently available for it.</summary>
    public sealed class TldEntry
    {
        /// <summary>Without the leading dot: "com.tr".</summary>
        public string Tld { get; init; } = string.Empty;
        public List<TldProviderPrice> Prices { get; init; } = new();
        /// <summary>URL slug: "com-tr".</summary>
        public string Slug => Tld.Replace('.', '-');
    }
}
