using DomainQueryEngine.Application.TldPricing;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed record OfferCellModel(PricedOffer? Offer, bool ShowProvider = false, bool ShowDetails = false, bool Highlight = false);
}
