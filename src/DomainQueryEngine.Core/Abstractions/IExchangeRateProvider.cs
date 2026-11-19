namespace DomainQueryEngine.Core.Abstractions
{
    public interface IExchangeRateProvider
    {
        /// <summary>
        /// Returns the current rates, or null when none could be loaded (prices are then shown unconverted).
        /// </summary>
        Task<ExchangeRates?> GetRatesAsync(CancellationToken ct = default);
    }
}
