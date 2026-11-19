namespace DomainQueryEngine.Core
{
    public class SearchResult
    {
        /// <summary>
        /// Provider details as the strategy defines them. <see cref="Core.ProviderInfo.VisitLink"/>
        /// stays a template (may contain <c>{0}</c>); use <see cref="VisitLink"/> for the final URL.
        /// </summary>
        public ProviderInfo ProviderInfo { get; set; } = new();
        public DomainResult Domain { get; set; } = new();

        /// <summary>
        /// The provider's visit link with the searched domain filled in.
        /// </summary>
        public string VisitLink { get; set; } = string.Empty;
    }
}
