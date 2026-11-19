using DomainQueryEngine.Infrastructure.ExchangeRates;
using DomainQueryEngine.Infrastructure.Test.Fixtures;

namespace DomainQueryEngine.Infrastructure.Test.Core
{
    public class TcmbExchangeRateProviderTests
    {
        [Fact]
        public void Parses_Forex_Selling_Rates_From_Real_Bulletin()
        {
            // Captured from https://www.tcmb.gov.tr/kurlar/today.xml on 19.11.2026.
            var rates = TcmbExchangeRateProvider.Parse(RecordedResponse.Read("tcmb-today.xml"));

            Assert.Equal(new DateTime(2026, 11, 19), rates.Date);
            Assert.Equal("TCMB", rates.Source);
            Assert.Equal(50.4694m, rates.TryPerUnit["USD"]);
            Assert.Equal(55.4579m, rates.TryPerUnit["EUR"]);
            Assert.Equal(1m, rates.TryPerUnit["TRY"]);
        }

        [Fact]
        public void Divides_By_Unit_For_Currencies_Quoted_Per_100()
        {
            var xml = """
                <Tarih_Date Date="11/19/2026">
                  <Currency CurrencyCode="JPY"><Unit>100</Unit><ForexSelling>32.50</ForexSelling></Currency>
                </Tarih_Date>
                """;

            Assert.Equal(0.325m, TcmbExchangeRateProvider.Parse(xml).TryPerUnit["JPY"]);
        }

        [Fact]
        public void Rejects_Document_Without_Rates()
        {
            Assert.Throws<FormatException>(() => TcmbExchangeRateProvider.Parse("<Tarih_Date Date=\"11/19/2026\" />"));
        }
    }

    public class ExchangeRatesTests
    {
        private static readonly DomainQueryEngine.Core.ExchangeRates Rates =
            new(new DateTime(2026, 11, 19), "TCMB", new Dictionary<string, decimal> { ["USD"] = 50m, ["EUR"] = 55m });

        [Theory]
        [InlineData(8.99, "USD", "TRY", 449.50)]
        [InlineData(449.50, "TRY", "USD", 8.99)]
        [InlineData(10, "USD", "EUR", 9.09)]
        [InlineData(10, "TRY", "TRY", 10)]
        public void Converts_Through_Lira(double amount, string from, string to, double expected)
        {
            Assert.True(Rates.TryConvert((decimal)amount, from, to, out var converted));
            Assert.Equal((decimal)expected, converted);
        }

        [Fact]
        public void Cannot_Convert_Unknown_Currency()
        {
            Assert.False(Rates.TryConvert(10m, "UNKNOWN", "TRY", out _));
        }
    }
}
