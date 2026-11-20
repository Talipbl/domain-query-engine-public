using DomainQueryEngine.Core;
using DomainQueryEngine.Http;
using System.Text;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.YoncuBilisim
{
    public class YoncuBilisimDomainAvailabilityProvider : IDomainAvailabilityProvider
    {
        private const string BaseUrl = "https://www.yoncu.com";
        private static readonly Uri HandshakeUri = new($"{BaseUrl}/tr");
        private static readonly Uri CheckUri = new($"{BaseUrl}/API/Domain/Check");

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly YoncuBilisimLookupResponseParser _parser;

        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }

        public YoncuBilisimDomainAvailabilityProvider(YoncuBilisimLookupResponseParser parser, IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
            _parser = parser;
            Provider = new ProviderInfo
            {
                Name = "Yöncü Bilişim",
                Url = "https://www.yoncu.com/",
                LogoUrl = "https://hostingadvice.net/wp-content/uploads/2022/05/yoncu.com_.webp",
                VisitLink = "https://www.yoncu.com/tr/domain/search/{0}"
            };
            Authentication = new AuthenticationInfo();
        }

        public Task HandshakeAsync() => HandshakeAsync(CancellationToken.None);

        private async Task HandshakeAsync(CancellationToken ct)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, HandshakeUri);
            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var token = YoncuBilisimLookupResponseParser.GetCfToken(await response.Content.ReadAsStringAsync(ct));
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("Yöncü page did not contain a cftoken");

            Authentication.Token = token;
        }

        public async Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(Authentication.Token))
                await HandshakeAsync(ct);

            try
            {
                return await CheckAsync(domainName, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException && !ct.IsCancellationRequested)
            {
                // The token may have expired: take a fresh one and try once more.
                await HandshakeAsync(ct);
                return await CheckAsync(domainName, ct);
            }
        }

        private async Task<DomainResult> CheckAsync(string domainName, CancellationToken ct)
        {
            var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Post, CheckUri);
            request.Headers.Add("Referer", $"{BaseUrl}/tr/domain/search");
            request.Headers.Add("cftoken", Authentication.Token);
            request.Content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { domain = domainName }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClientFactory.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync(ct);
            return _parser.ParseDomainResult(rawResponse)
                .FirstOrDefault(r => string.Equals(r.DomainName, domainName, StringComparison.OrdinalIgnoreCase))!;
        }
    }
}
