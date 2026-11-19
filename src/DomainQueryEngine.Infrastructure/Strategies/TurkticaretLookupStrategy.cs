using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Turkticaret
{
    /// <summary>
    /// Turkticaret.net answers POST /siberhosting/ajax/smartquery_v2.php without a session.
    /// .tr extensions go through a separate query type ("trquery") from the others ("dquery").
    /// </summary>
    public class TurkticaretLookupStrategy : IDomainLookupStrategy
    {
        private static readonly Uri LookupUri = new("https://www.turkticaret.net/siberhosting/ajax/smartquery_v2.php");

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TurkticaretStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public TurkticaretLookupStrategy(TurkticaretStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Turkticaret.net",
                Url = "https://www.turkticaret.net",
                LogoUrl = "https://www.turkticaret.net/new_assets_2015/img/logo.png",
                VisitLink = "https://www.turkticaret.net/domain-sorgulama"
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
            var isTrExtension = tld == "tr" || tld.EndsWith(".tr");

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, LookupUri);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://www.turkticaret.net/domain-sorgulama-sonuc");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["r"] = isTrExtension ? "trquery" : "dquery",
                ["q"] = sld,
                ["tld"] = "." + tld,
                ["qs"] = string.Empty,
                ["s"] = string.Empty,
                ["skip_main"] = "0"
            });

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            var results = _parser.ParseDomainResult(rawResponse);
            return results.FirstOrDefault(r => r.DomainName == domainName) ?? results.First();
        }
    }
}
