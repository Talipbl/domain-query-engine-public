using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class TurkticaretTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("turkticaret", "https://www.turkticaret.net/domain-fiyatlari", http, parser);
}
