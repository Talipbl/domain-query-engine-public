using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.GuzelHosting
{
    /// <summary>
    /// Güzel Hosting runs WHMCS; its domain checker answers GET orderdomain.php?action=checkAvailabilityOne
    /// without a session or token.
    /// </summary>
    public class GuzelHostingDomainAvailabilityProvider : IDomainAvailabilityProvider
    {
        private const string LookupUrl = "https://www.guzel.net.tr/orderdomain.php";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GuzelHostingLookupResponseParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public GuzelHostingDomainAvailabilityProvider(GuzelHostingLookupResponseParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Güzel Hosting",
                Url = "https://www.guzel.net.tr",
                LogoUrl = "https://www.guzel.net.tr/favicon.ico",
                VisitLink = "https://www.guzel.net.tr/orderdomain.php"
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

            var query = $"action=checkAvailabilityOne&domain={Uri.EscapeDataString(sld)}&tld={Uri.EscapeDataString("." + tld)}";
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, new Uri($"{LookupUrl}?{query}"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", LookupUrl);

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            return _parser.ParseDomainResult(rawResponse).First();
        }
    }
}
