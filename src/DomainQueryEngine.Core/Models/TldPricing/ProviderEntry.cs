namespace DomainQueryEngine.Core.TldPricing
{
    public sealed class ProviderEntry
    {
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Website { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public ProviderStatus Status { get; init; }
        public int? TldCount { get; init; }
    }
}
