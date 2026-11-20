using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class YoncuTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("yoncu-bilisim", "https://www.yoncu.com/tr/domain/prices", http, parser);
}
