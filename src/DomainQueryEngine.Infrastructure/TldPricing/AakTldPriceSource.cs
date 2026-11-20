using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class AakTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("alan-adi-kayit", "https://account.aak.com.tr/index.php?rp=%2Fdomain%2Fpricing", http, parser);
}
