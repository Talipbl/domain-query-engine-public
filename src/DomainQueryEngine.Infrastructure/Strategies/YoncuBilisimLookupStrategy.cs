using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using System.Net;
using System.Net.Http.Headers;

namespace DomainQueryEngine.YoncuBilisim
{
    public class YoncuBilisimLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly YoncuBilisimStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public YoncuBilisimLookupStrategy(YoncuBilisimStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Yöncü Bilişim",
                Url = "https://www.yoncu.com/",
                LogoUrl = "https://hostingadvice.net/wp-content/uploads/2022/05/yoncu.com_.webp",
                VisitLink = "https://www.yoncu.com/search/{0}"
            };
            Authentication = new AuthenticationInfo();
        }

        public async Task HandshakeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, new Uri($"https://www.yoncu.com/DomSorgula.php?domain={domainName}"));
            request.Headers.Add("Referer", "https://www.yoncu.com/");
            request.Headers.Add("Cookie", "2102F33EDC78CEEFB694569C84CFF468");

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
