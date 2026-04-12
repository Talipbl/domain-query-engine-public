using DomainQueryEngine.Core;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Natro
{
    public class NatroLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NatroStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public NatroLookupStrategy(NatroStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Natro",
                Url = "https://www.natro.com",
                LogoUrl = "https://s3.eu-central-1.amazonaws.com/bonusbay-images/campaign_banner/66e09ee5a53f56721556672737e24cb0.jpeg",
                VisitLink = "https://www.natro.com/domain-sorgulama"
            };
            Authentication = new AuthenticationInfo();
        }
        public Task HandshakeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.natro.com/domainnames/queryservicesap.asp"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://www.natro.com/domain-sorgulama");

            var collection = new List<KeyValuePair<string, string>>
                {
                    new("domainname", domainName),
                    new("format", "json")
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
                throw new HttpRequestException("Error occurred while sending request to Natro.", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
