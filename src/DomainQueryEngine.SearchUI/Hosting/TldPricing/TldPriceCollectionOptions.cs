namespace DomainQueryEngine.SearchUI.Hosting.TldPricing
{
    public sealed class TldPriceCollectionOptions
    {
        public const string SectionName = "PriceCollection";
        public int IntervalMinutes { get; set; } = 360;
        public int MaxConcurrency { get; set; } = 3;
    }
}
