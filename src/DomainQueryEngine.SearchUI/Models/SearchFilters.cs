namespace DomainQueryEngine.SearchUI.Models
{
    /// <summary>Filters on the search page. Defaults show every provider, cheapest first, shortest period.</summary>
    public sealed record SearchFilters(bool OnlyAvailable = false, int? Period = null, ResultSort Sort = ResultSort.Price)
    {
        public static readonly int[] Periods = { 1, 2, 5 };
        public bool IsDefault => !OnlyAvailable && Period == null && Sort == ResultSort.Price;
    }
}
