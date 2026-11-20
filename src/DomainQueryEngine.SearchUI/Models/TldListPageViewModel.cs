using DomainQueryEngine.Application.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class TldListPageViewModel
    {
        public static readonly IReadOnlyList<TldSortOption> Sorts = new TldSortOption[]
        {
            new("registration", "Tlds.Sort.Registration", TldSort.CheapestRegistration),
            new("renewal", "Tlds.Sort.Renewal", TldSort.CheapestRenewal),
            new("alphabetical", "Tlds.Sort.Alphabetical", TldSort.Alphabetical)
        };

        public TldPriceList List { get; init; } = new();
        public List<TldListItem> Items { get; init; } = new();
        public TldSortOption Sort { get; init; } = Sorts[2];
        public string Query { get; init; } = string.Empty;
        public bool AnyConverted => Items.Any(i => i.CheapestRegistration?.IsConverted == true || i.CheapestRenewal?.IsConverted == true);
    }
}
