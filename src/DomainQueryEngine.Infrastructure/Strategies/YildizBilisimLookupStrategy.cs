using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using DomainQueryEngine.Infrastructure.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static DomainQueryEngine.Http.StrategyConstants;

namespace DomainQueryEngine.YildizBilisim
{
    public class YildizBilisimLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly YildizBilisimStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public YildizBilisimLookupStrategy(YildizBilisimStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Yildiz Bilisim",
                Url = "https://www.yildizbilisim.com.tr",
                LogoUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSNvSCsHmeiKZNWRyiKNO_Rjzo0lg7Nj48rHA&s",
                VisitLink = "https://www.yildizbilisim.com.tr/cart.php?a=add&domain=register&query={0}"
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

            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.yildizbilisim.com.tr/index.php?rp=/domain/check"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", $"https://www.yildizbilisim.com.tr/cart.php?a=add&domain=register&query={domainName}");

            var collection = new List<KeyValuePair<string, string>>
                {
                    new("token", Authentication.Token),
                    new("domain", domainName),
                    new("ajax", "1"),
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
                throw new Exception("Error occurred while sending request to Yildiz Bilisim.", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
