using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Http;
using DomainQueryEngine.IsimTescil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Ihs
{
    public class IhsLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IhsStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public IhsLookupStrategy(IhsStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "ihs",
                Url = "https://www.ihs.com.tr/",
                LogoUrl = "https://www.ihs.com.tr/assets/images/header-menu/ihs-logo-new.svg",
                VisitLink = "https://www.ihs.com.tr/domain/alan-adi-domain-tescili.html"
            };
            Authentication = new AuthenticationInfo();
        }

        public Task HandshakeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.ihs.com.tr/domain/alan-adi-domain-tescili.html"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://www.ihs.com.tr/domain/alan-adi-domain-tescili.html");

            var collection = new List<KeyValuePair<string, string>>
                {
                    new("domainNameDisplay", domainName),
                    new("domainName", DomainHelper.GetDomainFromDomainName(domainName))
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
                throw new HttpRequestException("Error occurred while sending request to Ihs", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
