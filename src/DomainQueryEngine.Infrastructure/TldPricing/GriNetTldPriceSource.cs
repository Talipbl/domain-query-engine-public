using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class GriNetTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("grinet", "https://www.gri.net/domain-fiyatlari", http, parser);
}
