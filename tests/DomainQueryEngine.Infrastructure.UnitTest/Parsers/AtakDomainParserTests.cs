using DomainQueryEngine.AtakDomain;
using DomainQueryEngine.Infrastructure.Test.Fixtures;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real responses captured from the provider's site (November 2026).

    public class AtakDomainParserTests
    {
        private readonly AtakDomainStrategyParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_With_Campaign_Price()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("atak-available.json")));

            Assert.Equal("ornekyazilimci.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.True(result.IsValidDomain);

            // Price 8.99 is the campaign price, Selling 18.99 the regular one.
            var price = Assert.Single(result.Prices);
            Assert.Equal(1, price.Period);
            WhmcsParserTests.AssertPrice(8.99m, "USD", price.RegistrationPrice);
            WhmcsParserTests.AssertPrice(18.99m, "USD", price.RegularRegistrationPrice);
        }

        [Fact]
        public void Parses_Registered_Domain_As_Transferable()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("atak-taken.json")));

            Assert.Equal("hurriyet.com.tr", result.DomainName);
            Assert.Equal("com.tr", result.Tld);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsTransferable);

            var price = Assert.Single(result.Prices);
            Assert.Null(price.RegistrationPrice);
            WhmcsParserTests.AssertPrice(1.99m, "USD", price.TransferPrice);
        }

        [Fact]
        public void Rejects_Unsuccessful_Response()
        {
            Assert.Throws<ArgumentException>(() => _parser.ParseDomainResult("""{"Success":false,"Message":"Rate limit"}"""));
        }
    }
}
