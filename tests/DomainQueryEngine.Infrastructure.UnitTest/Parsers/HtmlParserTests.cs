using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Ihs;
using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.IsimTescil;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YildizBilisim;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // These parsers look up the TLD through DomainHelper, which downloads the public
    // suffix list once per test run.

    public class IhsParserTests
    {
        private readonly IhsLookupResponseParser _parser = new();

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
        private readonly IsimTescilLookupResponseParser _parser = new();

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
        private readonly YildizBilisimLookupResponseParser _parser = new();

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
