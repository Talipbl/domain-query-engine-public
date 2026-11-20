namespace DomainQueryEngine.Core.TldPricing
{
    /// <summary>A provider's current registration and renewal prices in the provider's own currency.</summary>
    public sealed class TldProviderPrice
    {
        public string Tld { get; init; } = string.Empty;
        public string Provider { get; init; } = string.Empty;
        public string Currency { get; init; } = "TRY";
        public decimal? Registration { get; init; }
        public decimal? RegularRegistration { get; init; }
        public int RegistrationTermYears { get; init; } = 1;
        public decimal? Renewal { get; init; }
        public int RenewalTermYears { get; init; } = 1;
        public bool IsCampaign => RegularRegistration is > 0 && Registration is > 0 && RegularRegistration > Registration;
    }
}
