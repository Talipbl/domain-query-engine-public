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

        /// <summary>
        /// Sends the request through the retry policy. An <see cref="HttpRequestMessage"/> can only be
        /// sent once, so every attempt sends a fresh copy built from the original (body buffered once).
        /// </summary>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(ct);

            return await _policy.ExecuteAsync(
                token => _httpClient.SendAsync(CloneRequest(request, body), token),
                ct);
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? body)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version,
                VersionPolicy = original.VersionPolicy
            };

            foreach (var header in original.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (body != null && original.Content != null)
            {
                clone.Content = new ByteArrayContent(body);
                foreach (var header in original.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Add("User-Agent", HttpClientConstants.UserAgent);
            return request;
        }
    }
}
