using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turhost
{
    /// <summary>
    /// Turhost's panel exposes a public JSONP endpoint: GET /api/v1/domain/query?domain=sld&amp;tlds[]=tld.
    /// </summary>
    public class TurhostDomainAvailabilityProvider : IDomainAvailabilityProvider
    {
        private const string LookupUrl = "https://panel.turhost.com/api/v1/domain/query";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TurhostLookupResponseParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public TurhostDomainAvailabilityProvider(TurhostLookupResponseParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Turhost",
                Url = "https://www.turhost.com",
                LogoUrl = "https://www.turhost.com/assets/img/logo.png",
                VisitLink = "https://www.turhost.com/domain/domain-sorgulama/?domain={0}"
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
            var tld = DomainHelper.GetTldFromDomainName(domainName);
            var sld = domainName.Substring(0, domainName.Length - tld.Length).TrimEnd('.');

            var query = $"domain={Uri.EscapeDataString(sld)}&{Uri.EscapeDataString("tlds[]")}={Uri.EscapeDataString(tld)}";
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, new Uri($"{LookupUrl}?{query}"));
            request.Headers.Add("Referer", "https://www.turhost.com/domain/domain-sorgulama/");

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            var results = _parser.ParseDomainResult(rawResponse);
            return results.FirstOrDefault(r => r.DomainName == domainName) ?? results.First();
        }
    }
}
