using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using static DomainQueryEngine.Http.StrategyConstants;

namespace DomainQueryEngine.IsimKayit
{
    public class IsimKayitLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IsimKayitStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public IsimKayitLookupStrategy(IsimKayitStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "İsim Kayıt",
                Url = "https://www.isimkayit.com/",
                LogoUrl = "https://www.isimkayit.com/templates/ikmc/img/logo.png",
                VisitLink = "https://www.isimkayit.com/alan-adi-kaydi?query={0}"
            };
            Authentication = new AuthenticationInfo();
        }

        public async Task HandshakeAsync()
        {
            var response = await _client.GetAsync(Provider.Url);
            response.EnsureSuccessStatusCode();

            var htmlResponse = await response.Content.ReadAsStringAsync();
            var token = _parser.GetCsrfTokenFromHtml(htmlResponse);

            Authentication.Token = token;
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(Authentication.Token))
                await HandshakeAsync();

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.isimkayit.com/index.php?rp=/domain/check"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", $"https://www.atakdomain.com/domain-sorgulama?s={domainName}");

            var collection = new List<KeyValuePair<string, string>>
                {
                    new("token", Authentication.Token),
                    new("domain", domainName),
                    new("a", "checkDomain"),
                    new("type", QueryType.domain)
                 };

            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await _httpClientFactory.SendAsync(request, ct);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                await HandshakeAsync();
                throw new HttpRequestException("Error occurred while sending request to Isim Kayit", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
