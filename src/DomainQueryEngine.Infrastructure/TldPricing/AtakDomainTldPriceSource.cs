using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class AtakDomainTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("atak-domain", "https://atakdomain.com/domain-fiyatlari", http, parser);
}
