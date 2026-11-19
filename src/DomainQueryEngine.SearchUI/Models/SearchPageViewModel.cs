namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class SearchPageViewModel
    {
        /// <summary>What the visitor typed.</summary>
        public string Input { get; init; } = string.Empty;

        /// <summary>The normalized domain; empty when <see cref="ErrorKey"/> is set.</summary>
        public string Domain { get; init; } = string.Empty;
        /// <summary>Localization key of the input error; null when the input is valid.</summary>
        public string? ErrorKey { get; init; }
        public SearchFilters Filters { get; init; } = new();
    }
}
