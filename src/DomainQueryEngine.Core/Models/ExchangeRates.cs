namespace DomainQueryEngine.Core
{
    /// <summary>
    /// Exchange rates expressed as Turkish lira per one unit of each currency (TRY itself is 1).
    /// </summary>
    public sealed class ExchangeRates
    {
        public const string BaseCurrency = "TRY";

        private readonly Dictionary<string, decimal> _tryPerUnit;

        public ExchangeRates(DateTime date, string source, IDictionary<string, decimal> tryPerUnit)
        {
            Date = date;
            Source = source;
            _tryPerUnit = new Dictionary<string, decimal>(tryPerUnit, StringComparer.OrdinalIgnoreCase)
            {
                [BaseCurrency] = 1m
            };
        }

        /// <summary>The day the rates were published.</summary>
        public DateTime Date { get; }

        /// <summary>Where the rates come from, e.g. "TCMB".</summary>
        public string Source { get; }

        public IReadOnlyDictionary<string, decimal> TryPerUnit => _tryPerUnit;

        /// <summary>
        /// Converts <paramref name="amount"/> between two currencies through TRY.
        /// Returns false when either currency has no rate.
        /// </summary>
        public bool TryConvert(decimal amount, string from, string to, out decimal converted)
        {
            converted = 0;
            if (!_tryPerUnit.TryGetValue(from, out var fromRate) || !_tryPerUnit.TryGetValue(to, out var toRate) || toRate <= 0)
                return false;

            converted = decimal.Round(amount * fromRate / toRate, 2);
            return true;
        }
    }
}
