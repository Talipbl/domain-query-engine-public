using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using static DomainQueryEngine.Http.StrategyConstants;

namespace DomainQueryEngine.AAK
{
    public class AlanAdiKayitLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AlanAdiKayitStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public AlanAdiKayitLookupStrategy(AlanAdiKayitStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Alan Adı Kayıt",
                Url = "https://account.aak.com.tr",
                VisitLink = "https://account.aak.com.tr/cart.php?a=add&domain=register",
                LogoUrl = "https://aak.com.tr/img/logo.png"
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

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://account.aak.com.tr/index.php?rp=/domain/check"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", $"https://account.aak.com.tr/cart.php?a=add&domain=register");

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
            catch (Exception ex)
            {
                await HandshakeAsync();
                throw new HttpRequestException("Error occurred while sending request to Alan Adı Kayıt.", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
