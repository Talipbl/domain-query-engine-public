using DomainQueryEngine.Infrastructure.DomainSearch.Providers.GuzelHosting;
using DomainQueryEngine.Infrastructure.Test.Fixtures;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real responses captured from the provider's site (November 2026).

    public class GuzelHostingParserTests
    {
        private readonly GuzelHostingLookupResponseParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_With_All_Periods()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("guzel-available.json")));

            Assert.Equal("ornekyazilimci.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.Equal(Enumerable.Range(1, 10), result.Prices.Select(p => p.Period));
            WhmcsParserTests.AssertPrice(504.29m, "TRY", result.Prices[0].RegistrationPrice);
            WhmcsParserTests.AssertPrice(8450.28m, "TRY", result.Prices[9].RegistrationPrice);
        }

        [Fact]
        public void Parses_Taken_Domain()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("guzel-taken.json")));

            Assert.Equal("hurriyet.com.tr", result.DomainName);
            Assert.Equal("com.tr", result.Tld);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Rejects_Non_Json_Response()
        {
            Assert.Throws<ArgumentException>(() => _parser.ParseDomainResult("<html>error</html>"));
        }
    }
}
