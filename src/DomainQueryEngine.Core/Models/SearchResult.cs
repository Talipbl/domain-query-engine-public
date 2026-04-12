namespace DomainQueryEngine.Core
{
    public class SearchResult
    {
        public ProviderInfo ProviderInfo { get; set; } = new();
        public DomainResult Domain { get; set; } = new();
    }
}
