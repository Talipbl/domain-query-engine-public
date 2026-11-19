using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.Turkticaret;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real responses captured from the provider's site (November 2026).

    public class TurkticaretParserTests
    {
        private readonly TurkticaretStrategyParser _parser = new();

        [Theory]
        [InlineData("turkticaret-available.json", "ornekyazilimci.com", "com", 128.99)]
        [InlineData("turkticaret-tr-available.json", "ornekyazilimci.com.tr", "com.tr", 50.99)]
        public void Parses_Available_Domain_With_Campaign_Price(string fixture, string domain, string tld, double price)
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read(fixture)));

            Assert.Equal(domain, result.DomainName);
            Assert.Equal(tld, result.Tld);
            Assert.True(result.IsAvailable);

            // The first-purchase campaign price is lower than the regular one ("campaign_price").
            var only = Assert.Single(result.Prices);
            WhmcsParserTests.AssertPrice((decimal)price, "TRY", only.RegistrationPrice);
        }

        [Theory]
        [InlineData("turkticaret-taken.json", "google.com")]
        [InlineData("turkticaret-tr-taken.json", "hurriyet.com.tr")]
        public void Parses_Registered_Domain_Without_Prices(string fixture, string domain)
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read(fixture)));

            Assert.Equal(domain, result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Fails_On_Security_Challenge_Instead_Of_Solving_It()
        {
            Assert.Throws<InvalidOperationException>(() => _parser.ParseDomainResult(TurkticaretStrategyParser.SecurityChallengeResponse));
        }

        [Fact]
        public void Reads_Main_Dom_Keyed_By_Domain()
        {
            var raw = """{"main_dom":{"a.com.tr":{"domain_utf8":"a.com.tr","status":"available","price_raw":50.99,"currency":"&#x20BA","tld":".com.tr"}}}""";

            var result = Assert.Single(_parser.ParseDomainResult(raw));

            Assert.Equal("a.com.tr", result.DomainName);
            WhmcsParserTests.AssertPrice(50.99m, "TRY", Assert.Single(result.Prices).RegistrationPrice);
        }
    }
}
