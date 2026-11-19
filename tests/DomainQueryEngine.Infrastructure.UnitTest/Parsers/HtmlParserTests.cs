using DomainQueryEngine.Ihs;
using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.IsimTescil;
using DomainQueryEngine.YildizBilisim;
using DomainQueryEngine.YoncuBilisim;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // These parsers look up the TLD through DomainHelper, which downloads the public
    // suffix list once per test run.

    public class YoncuBilisimParserTests
    {
        private readonly YoncuBilisimStrategyParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_With_All_Periods()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.YoncuAvailable));

            Assert.Equal("salihotoyikama2.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.False(result.IsTransferable);
            Assert.Equal(Enumerable.Range(1, 10), result.Prices.Select(p => p.Period));
            WhmcsParserTests.AssertPrice(580m, "TRY", result.Prices[0].RegistrationPrice);
            WhmcsParserTests.AssertPrice(1260m, "TRY", result.Prices[1].RegistrationPrice);
            WhmcsParserTests.AssertPrice(6700m, "TRY", result.Prices[9].RegistrationPrice);
        }

        [Fact]
        public void Parses_Taken_Domain()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.YoncuTaken));

            Assert.Equal("google.com", result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.Empty(result.Prices);
        }
    }

    public class IhsParserTests
    {
        private readonly IhsStrategyParser _parser = new();

        [Fact]
        public void Parses_Only_Available_Rows_With_Tl_Price()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.IhsAvailable));

            Assert.Equal("ornek.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);

            var price = Assert.Single(result.Prices);
            Assert.Equal(1, price.Period);
            WhmcsParserTests.AssertPrice(449.90m, "TRY", price.RegistrationPrice);
        }

        [Fact]
        public void Skips_Price_When_Tl_Price_Is_Missing()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.IhsWithoutTlPrice));

            Assert.Equal("ornek.net", result.DomainName);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Returns_Empty_List_For_Unrelated_Html()
        {
            Assert.Empty(_parser.ParseDomainResult("<html><body>bakım</body></html>"));
        }
    }

    public class IsimTescilParserTests
    {
        private readonly IsimTescilStrategyParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_And_Ignores_Old_Price()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.IsimTescilAvailable));

            Assert.Equal("ornek.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);

            var price = Assert.Single(result.Prices);
            Assert.Equal(1, price.Period);
            WhmcsParserTests.AssertPrice(399m, "TRY", price.RegistrationPrice);
        }

        [Fact]
        public void Returns_Empty_List_When_There_Are_No_Rows()
        {
            Assert.Empty(_parser.ParseDomainResult("<div>sonuç yok</div>"));
        }
    }

    public class CsrfTokenTests
    {
        private readonly YildizBilisimStrategyParser _parser = new();

        [Fact]
        public void Reads_Token_From_Script()
        {
            var html = "<html><script>var x = 1;</script><script>var csrfToken = 'abc123';</script></html>";

            Assert.Equal("abc123", _parser.GetCsrfTokenFromHtml(html));
        }

        [Fact]
        public void Returns_Empty_When_Page_Has_No_Scripts()
        {
            Assert.Equal(string.Empty, _parser.GetCsrfTokenFromHtml("<html><body></body></html>"));
        }
    }
}
