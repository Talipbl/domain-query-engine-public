using System.Globalization;
using System.Xml.Linq;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Http;

namespace DomainQueryEngine.Infrastructure.ExchangeRates
{
    /// <summary>
    /// Reads the Central Bank of the Republic of Türkiye (TCMB) daily rates (forex selling) and keeps
    /// them for two hours. If TCMB cannot be reached, the last rates loaded are used; with none at all,
    /// <see cref="GetRatesAsync"/> returns null and prices are shown unconverted.
    /// </summary>
    public sealed class TcmbExchangeRateProvider : IExchangeRateProvider, IDisposable
    {
        public const string Source = "TCMB";
        private static readonly Uri TodayUri = new("https://www.tcmb.gov.tr/kurlar/today.xml");
        private static readonly TimeSpan RefreshAfter = TimeSpan.FromHours(2);
        private static readonly TimeSpan RetryAfterFailure = TimeSpan.FromMinutes(5);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private Core.ExchangeRates? _rates;
        private DateTime _nextRefreshUtc = DateTime.MinValue;

        public TcmbExchangeRateProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Core.ExchangeRates?> GetRatesAsync(CancellationToken ct = default)
        {
            if (DateTime.UtcNow < _nextRefreshUtc)
                return _rates;

            await _refreshLock.WaitAsync(ct);
            try
            {
                if (DateTime.UtcNow < _nextRefreshUtc)
                    return _rates;

                try
                {
                    var request = _httpClientFactory.CreateHttpRequestMessage(HttpMethod.Get, TodayUri);
                    var response = await _httpClientFactory.SendAsync(request, ct);
                    response.EnsureSuccessStatusCode();

                    _rates = Parse(await response.Content.ReadAsStringAsync(ct));
                    _nextRefreshUtc = DateTime.UtcNow + RefreshAfter;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Keep the last rates (if any) and try again a little later.
                    _nextRefreshUtc = DateTime.UtcNow + RetryAfterFailure;
                }

                return _rates;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>
        /// Parses today.xml: one &lt;Currency CurrencyCode="USD"&gt; per currency with &lt;Unit&gt; and &lt;ForexSelling&gt;.
        /// </summary>
        public static Core.ExchangeRates Parse(string xml)
        {
            var root = XDocument.Parse(xml).Root
                ?? throw new FormatException("TCMB rates document is empty.");

            var date = DateTime.TryParseExact((string?)root.Attribute("Date"), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : DateTime.UtcNow.Date;

            var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var currency in root.Elements("Currency"))
            {
                var code = (string?)currency.Attribute("CurrencyCode");
                if (string.IsNullOrEmpty(code)
                    || !decimal.TryParse((string?)currency.Element("ForexSelling"), NumberStyles.Number, CultureInfo.InvariantCulture, out var selling)
                    || selling <= 0)
                    continue;

                var unit = decimal.TryParse((string?)currency.Element("Unit"), NumberStyles.Number, CultureInfo.InvariantCulture, out var u) && u > 0 ? u : 1m;
                rates[code] = selling / unit;
            }

            if (rates.Count == 0)
                throw new FormatException("TCMB rates document has no forex selling rates.");

            return new Core.ExchangeRates(date, Source, rates);
        }

        public void Dispose() => _refreshLock.Dispose();
    }
}
