using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using System;
using static DomainQueryEngine.Http.StrategyConstants;

namespace DomainQueryEngine.GriNet
{
    public class GriNetLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GriNetStrategyParser _parser;

        public ProviderInfo Provider { get ; set; }
        public AuthenticationInfo Authentication { get; set; }

        public GriNetLookupStrategy(GriNetStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "GriNet",
                Url = "https://gri.net",
                LogoUrl = "https://www.find.com.tr/assets/images/Uploads/Company/1752566/find_20181231_102346364_griinternettek.jpg",
                VisitLink = "https://www.gri.net/tr/domain-kayit?domain={0}"
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

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.gri.net/tr/cart"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", $"https://www.gri.net/tr/domain-kayit?domain={domainName}");

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
                throw new HttpRequestException("Error occurred while sending request to GriNet", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
