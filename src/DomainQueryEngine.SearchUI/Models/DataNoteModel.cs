using DomainQueryEngine.Core;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed record DataNoteModel(DateTime AsOf, ExchangeRates? Rates, bool AnyConverted);
}
