using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Http
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly string proxyUrl = "http://185.18.250.194:80";
        private readonly HttpClientHandler _handler = new()
        {
            //Proxy = new WebProxy(proxyUrl),
            //UseProxy = true,
            CookieContainer = new CookieContainer(),
            UseCookies = true,
            MaxConnectionsPerServer = 500,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public HttpClientFactory(IAsyncPolicy<HttpResponseMessage> policy)
        {
            _httpClient = new HttpClient(_handler, disposeHandler: false);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _policy = policy;
        }

        public HttpClient CreateClient() => _httpClient;

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
       => _policy.ExecuteAsync(() => _httpClient.SendAsync(request, ct));

        public HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Add("User-Agent", HttpClientConstants.UserAgent);
            return request;
        }
    }
}
