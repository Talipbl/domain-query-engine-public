namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class SearchPageViewModel
    {
        /// <summary>What the visitor typed.</summary>
        public string Input { get; init; } = string.Empty;

        /// <summary>The normalized domain; empty when <see cref="Error"/> is set.</summary>
        public string Domain { get; init; } = string.Empty;
        public string? Error { get; init; }
        public SearchFilters Filters { get; init; } = new();
    }
}
