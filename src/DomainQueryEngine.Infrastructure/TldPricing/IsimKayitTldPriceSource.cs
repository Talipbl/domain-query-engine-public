using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    public sealed class IsimKayitTldPriceSource(IHttpClientFactory http, TldPriceTableParser parser)
        : HtmlTableTldPriceSource("isim-kayit", "https://www.isimkayit.com/alan-adi-uzantilari", http, parser);
}
