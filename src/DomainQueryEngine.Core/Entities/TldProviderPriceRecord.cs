namespace DomainQueryEngine.Core.Entities
{
    /// <summary>The latest published price for one extension at one provider.</summary>
    public sealed class TldProviderPriceRecord
    {
        public string ProviderSlug { get; set; } = string.Empty;
        public string Tld { get; set; } = string.Empty;
        public string Currency { get; set; } = "TRY";
        public decimal? Registration { get; set; }
        public decimal? RegularRegistration { get; set; }
        public int RegistrationTermYears { get; set; } = 1;
        public decimal? Renewal { get; set; }
        public int RenewalTermYears { get; set; } = 1;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime ObservedAt { get; set; }
    }

    /// <summary>Collection health and last successful import for one provider.</summary>
    public sealed class TldPriceSyncRecord
    {
        public string ProviderSlug { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime LastAttemptAt { get; set; }
        public DateTime? LastSuccessAt { get; set; }
        public int PriceCount { get; set; }
        public string? LastError { get; set; }
    }
}
