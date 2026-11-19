using System.Text;
using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using Newtonsoft.Json;

namespace DomainQueryEngine.AtakDomain
{
    /// <summary>
    /// Atak Domain exposes a JSON availability endpoint that needs no session or token.
    /// </summary>
    public class AtakDomainLookupStrategy : IDomainLookupStrategy
    {
        private static readonly Uri LookupUri = new("https://www.atakdomain.com/api/domain/getavailability");

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AtakDomainStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public AtakDomainLookupStrategy(AtakDomainStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Atak Domain",
                Url = "https://www.atakdomain.com",
                LogoUrl = "https://www.atakdomain.com/assets/img/brand/atak-domain-hosting-logo.svg",
                VisitLink = "https://www.atakdomain.com/domain-sorgulama?s={0}"
            };
            Authentication = new AuthenticationInfo();
        }

        public Task HandshakeAsync()
        {
            // This provider needs no session or CSRF token.
            return Task.CompletedTask;
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, LookupUri);
            request.Headers.Add("Referer", "https://www.atakdomain.com/domain-sorgulama");
            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { domain = domainName }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            return _parser.ParseDomainResult(rawResponse).First();
        }
    }
}
