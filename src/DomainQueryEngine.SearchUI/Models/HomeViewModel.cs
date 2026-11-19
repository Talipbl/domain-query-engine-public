namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class HomeViewModel
    {
        /// <summary>Domain from ?q=, searched on page load.</summary>
        public string Query { get; init; } = string.Empty;

        /// <summary>Names of the providers that are queried, in Turkish alphabetical order.</summary>
        public List<string> ProviderNames { get; init; } = new();
    }
}
