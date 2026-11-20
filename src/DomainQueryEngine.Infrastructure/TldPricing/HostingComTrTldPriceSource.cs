using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class HostingComTrTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("hosting-com-tr", "https://www.hosting.com.tr/domain/domain-transferi/", http, parser);
}
