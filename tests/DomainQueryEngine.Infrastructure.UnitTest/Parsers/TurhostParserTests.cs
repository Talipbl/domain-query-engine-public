using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.Turhost;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real responses captured from the provider's site (November 2026).

    public class TurhostParserTests
    {
        private readonly TurhostStrategyParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_Preferring_Campaign_Prices()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("turhost-available.jsonp")));

            Assert.Equal("ornekyazilimci.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Prices.Select(p => p.Period));

            // Regular 1-year TRY price is 503.90, the campaign price 146.36.
            WhmcsParserTests.AssertPrice(146.36m, "TRY", result.Prices[0].RegistrationPrice);
            WhmcsParserTests.AssertPrice(503.90m, "TRY", result.Prices[0].RegularRegistrationPrice);
            WhmcsParserTests.AssertPrice(1256.20m, "TRY", result.Prices[1].RegistrationPrice);
        }

        [Fact]
        public void Parses_Registered_Domain_Without_Prices()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("turhost-taken.jsonp")));

            Assert.Equal("hurriyet.com.tr", result.DomainName);
            Assert.Equal("com.tr", result.Tld);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Parses_Domain_That_Cannot_Be_Registered()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("turhost-unable.jsonp")));

            Assert.Equal("google.com", result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.False(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Theory]
        [InlineData("/**/callback({\"a\":1});", "{\"a\":1}")]
        [InlineData("{\"a\":1}", "{\"a\":1}")]
        public void Strips_Jsonp_Wrapper(string input, string expected)
        {
            Assert.Equal(expected, TurhostStrategyParser.StripJsonp(input));
        }
    }
}
