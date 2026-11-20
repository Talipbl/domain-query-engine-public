using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    /// <summary>Shared HTTP handling for providers that publish an HTML price table.</summary>
    public abstract class HtmlTableTldPriceSource : ITldPriceSource
    {
        private readonly IHttpClientFactory _http;
        private readonly TldPriceTableParser _parser;

        protected HtmlTableTldPriceSource(string providerSlug, string sourceUrl, IHttpClientFactory http, TldPriceTableParser parser)
        {
            ProviderSlug = providerSlug;
            SourceUrl = sourceUrl;
            _http = http;
            _parser = parser;
        }

        public string ProviderSlug { get; }
        public string SourceUrl { get; }

        public async Task<TldPriceSnapshot> FetchAsync(CancellationToken ct = default)
        {
            using var request = _http.CreateHttpRequestMessage(HttpMethod.Get, new Uri(SourceUrl));
            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync(ct);
            var prices = _parser.Parse(html, ProviderSlug);
            return new TldPriceSnapshot
            {
                ProviderSlug = ProviderSlug,
                SourceUrl = SourceUrl,
                ObservedAt = DateTime.UtcNow,
                Prices = prices
            };
        }
    }
}
