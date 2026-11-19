using DomainQueryEngine.Core.Utilities;

namespace DomainQueryEngine.Infrastructure.Test.Core
{
    public class PriceHelperTests
    {
        [Theory]
        [InlineData("₺4000.00 TL", 4000.00, "TRY")]
        [InlineData("580 ₺", 580, "TRY")]
        [InlineData("449,90 TL", 449.90, "TRY")]
        [InlineData("1.234,56 TL", 1234.56, "TRY")]
        [InlineData("$1,234.56", 1234.56, "USD")]
        [InlineData("12.99 USD", 12.99, "USD")]
        [InlineData("€ 9,50", 9.50, "EUR")]
        [InlineData("£7", 7, "GBP")]
        [InlineData("15.00", 15.00, "UNKNOWN")]
        public void Parses_Amount_And_Currency(string input, double amount, string currency)
        {
            var (parsedAmount, parsedCurrency) = PriceHelper.Parse(input);

            Assert.Equal((decimal)amount, parsedAmount);
            Assert.Equal(currency, parsedCurrency);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Returns_Zero_For_Empty_Input(string? input)
        {
            Assert.Equal((0m, "UNKNOWN"), PriceHelper.Parse(input!));
        }

        [Fact]
        public void Returns_Zero_When_There_Is_No_Number()
        {
            Assert.Equal((0m, "TRY"), PriceHelper.Parse("Ücretsiz TL"));
        }

        [Theory]
        [InlineData("TRY", "₺")]
        [InlineData("usd", "$")]
        [InlineData("EUR", "€")]
        [InlineData("JPY", "JPY")]
        [InlineData("", "")]
        public void Maps_Currency_Code_To_Symbol(string code, string symbol)
        {
            Assert.Equal(symbol, PriceHelper.GetCurrencySymbol(code));
        }
    }

    /// <summary>
    /// DomainHelper downloads the public suffix list once per test run, so these tests need network access.
    /// </summary>
    public class DomainHelperTests
    {
        [Theory]
        [InlineData("example.com", "example.com")]
        [InlineData("  Example.COM ", "example.com")]
        [InlineData("https://www.example.com/", "example.com")]
        [InlineData("http://shop.example.com.tr/path", "example.com.tr")]
        [InlineData("www.ornek.co.uk", "ornek.co.uk")]
        [InlineData("example.com?ref=x#top", "example.com")]
        public void Normalizes_To_Registrable_Domain(string input, string expected)
        {
            Assert.Equal(expected, DomainHelper.ValidateAndNormalizeDomain(input));
        }

        [Fact]
        public void Appends_Com_When_Input_Has_No_Known_Suffix()
        {
            Assert.Equal("ornek.com", DomainHelper.ValidateAndNormalizeDomain("ornek"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Rejects_Empty_Input(string input)
        {
            Assert.Throws<ArgumentException>(() => DomainHelper.ValidateAndNormalizeDomain(input));
        }

        [Theory]
        [InlineData("example.com", "com")]
        [InlineData("example.com.tr", "com.tr")]
        [InlineData("www.example.co.uk", "co.uk")]
        public void Gets_Top_Level_Domain(string input, string expected)
        {
            Assert.Equal(expected, DomainHelper.GetTldFromDomainName(input));
        }

        [Fact]
        public void Gets_Domain_Label()
        {
            Assert.Equal("example", DomainHelper.GetDomainFromDomainName("www.example.com.tr"));
        }
    }
}
