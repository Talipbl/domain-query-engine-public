using DomainQueryEngine.Application.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed record TldSortOption(string Key, string LabelKey, TldSort Sort);
}
