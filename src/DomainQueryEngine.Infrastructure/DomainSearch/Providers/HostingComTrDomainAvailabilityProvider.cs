using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using HtmlAgilityPack;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.HostingComTr
{
    /// <summary>
    /// Hosting.com.tr needs a session: the search page sets the cookie and puts a CSRF token in
    /// #domainLookupForm, which POST /web/domain/querynew/lookup must send back.
    /// </summary>
    public class HostingComTrDomainAvailabilityProvider : IDomainAvailabilityProvider
    {
        private const string SearchPageUrl = "https://www.hosting.com.tr/domain/domain-sorgulama/";
        private static readonly Uri LookupUri = new("https://www.hosting.com.tr/web/domain/querynew/lookup");

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HostingComTrLookupResponseParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public HostingComTrDomainAvailabilityProvider(HostingComTrLookupResponseParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Hosting.com.tr",
                Url = "https://www.hosting.com.tr",
                LogoUrl = "https://www.hosting.com.tr/templates/battani/img/hosting-logo.svg",
                VisitLink = SearchPageUrl
            };
            Authentication = new AuthenticationInfo();
        }

        public async Task HandshakeAsync()
        {
            await HandshakeAsync(CancellationToken.None);
        }

        private async Task HandshakeAsync(CancellationToken ct)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, new Uri(SearchPageUrl));
            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(ct);
            Authentication.Token = ReadFormToken(html) ?? _parser.GetCsrfTokenFromHtml(html);

            if (string.IsNullOrEmpty(Authentication.Token))
                throw new InvalidOperationException("Hosting.com.tr search page did not contain a CSRF token.");
        }

        private static string? ReadFormToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode
                .SelectSingleNode("//form[@id='domainLookupForm']//input[@name='token']")
                ?.GetAttributeValue("value", null);
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(Authentication.Token))
                await HandshakeAsync(ct);

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, LookupUri);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", SearchPageUrl);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = Authentication.Token,
                ["lookupDomain"] = domainName
            });

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            var results = _parser.ParseDomainResult(rawResponse);

            // The response also lists alternative extensions; return the one that was asked for.
            return results.FirstOrDefault(r => r.DomainName == domainName)
                ?? throw new InvalidOperationException($"Hosting.com.tr returned no result for {domainName}.");
        }
    }
}
