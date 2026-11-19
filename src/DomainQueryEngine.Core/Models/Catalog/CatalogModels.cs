namespace DomainQueryEngine.Core.Catalog
{
    public enum TldCategory
    {
        /// <summary>Generic extensions such as .com, .net, .org.</summary>
        Generic,

        /// <summary>Turkish extensions run by TRABİS (.tr, .com.tr, ...).</summary>
        Turkish,

        /// <summary>Newer generic extensions (.xyz, .online, .app, ...).</summary>
        New,

        /// <summary>Other countries' extensions (.io, .co, ...).</summary>
        Country
    }

    /// <summary>
    /// An extension with each provider's prices as the provider quotes them (own currency).
    /// </summary>
    public sealed class TldEntry
    {
        /// <summary>Without the leading dot: "com.tr".</summary>
        public string Tld { get; init; } = string.Empty;
        public TldCategory Category { get; init; }
        public bool IsPopular { get; init; }

        /// <summary>True when documents are required to register; null when unknown.</summary>
        public bool? DocumentRequired { get; init; }

        /// <summary>Registry or authority, e.g. "TRABİS".</summary>
        public string? Registry { get; init; }

        /// <summary>Turkish description; <see cref="DescriptionEn"/> is the English one.</summary>
        public string? Description { get; init; }
        public string? DescriptionEn { get; init; }
        public List<TldProviderPrice> Prices { get; init; } = new();

        /// <summary>Lowest 1-year registration price per month, in TRY, oldest first.</summary>
        public List<PricePoint> History { get; init; } = new();
        public List<FaqEntry> Faq { get; init; } = new();

        /// <summary>URL slug: "com-tr".</summary>
        public string Slug => Tld.Replace('.', '-');
    }

    /// <summary>One provider's 1-year prices for an extension, in the provider's currency.</summary>
    public sealed class TldProviderPrice
    {
        /// <summary>Matches <see cref="ProviderEntry.Slug"/>.</summary>
        public string Provider { get; init; } = string.Empty;
        public string Currency { get; init; } = "TRY";

        /// <summary>The price to compare: the campaign price when there is one.</summary>
        public decimal? Registration { get; init; }

        /// <summary>Regular registration price when <see cref="Registration"/> is a campaign price.</summary>
        public decimal? RegularRegistration { get; init; }
        public decimal? Renewal { get; init; }
        public decimal? Transfer { get; init; }

        public bool IsCampaign => RegularRegistration is > 0 && Registration is > 0 && RegularRegistration > Registration;
    }

    public sealed record PricePoint(string Month, decimal Amount);

    /// <summary>A question and answer in Turkish, with optional English versions.</summary>
    public sealed record FaqEntry(string Question, string Answer, string? QuestionEn = null, string? AnswerEn = null);

    public enum ProviderStatus
    {
        Active,

        /// <summary>Registered but currently failing (e.g. the endpoint moved).</summary>
        Failing,

        /// <summary>Not queried (e.g. blocked by bot protection).</summary>
        Disabled
    }

    public sealed class ProviderEntry
    {
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Website { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public ProviderStatus Status { get; init; }

        /// <summary>Number of extensions the provider sells.</summary>
        public int? TldCount { get; init; }
    }

    public sealed record PopularQuery(string DomainName, int Count);

    /// <param name="Tld">Without the leading dot: "com.tr".</param>
    public sealed record PopularTld(string Tld, int Count)
    {
        public string Slug => Tld.Replace('.', '-');
    }
}
