using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class TldDetailPageViewModel(TldPriceDetail detail)
    {
        public TldPriceDetail Detail { get; } = detail;
        public TldEntry Entry => Detail.Entry;
        public bool AnyConverted => Detail.Rows.Any(r => r.Registration?.IsConverted == true || r.Renewal?.IsConverted == true);
    }
}
