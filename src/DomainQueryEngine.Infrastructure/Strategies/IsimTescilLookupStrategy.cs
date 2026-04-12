using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using DomainQueryEngine.YildizBilisim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DomainQueryEngine.Http.StrategyConstants;

namespace DomainQueryEngine.IsimTescil
{
    public class IsimTescilLookupStrategy : IDomainLookupStrategy
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IsimTescilStrategyParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }
        public IsimTescilLookupStrategy(IsimTescilStrategyParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _client = clientFactory.CreateClient();
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "İsim Tescil",
                Url = "https://www.isimtescil.net",
                LogoUrl = "https://www.isimtescil.net/Default/assets/images/logo2.png",
                VisitLink = "https://www.isimtescil.net/domain/domain-sorgula"
            };
            Authentication = new AuthenticationInfo();
        }

        public Task HandshakeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, new Uri("https://www.isimtescil.net/domain/search"));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://www.isimtescil.net/domain/search");

            var collection = new List<KeyValuePair<string, string>>
                {
                    new("domainname", domainName),
                    new("queryType", "6")
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
                throw new HttpRequestException("Error occurred while sending request to Isim Tescil.", ex);
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            var parseResult = _parser.ParseDomainResult(rawResponse);
            var searchResult = parseResult.FirstOrDefault(r => r.DomainName == domainName);

            return searchResult;
        }
    }
}
