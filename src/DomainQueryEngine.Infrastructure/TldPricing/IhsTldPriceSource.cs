using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class IhsTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("ihs", "https://www.ihs.com.tr/domain/domain-fiyatlari.html", http, parser);
}
