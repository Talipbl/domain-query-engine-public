using DomainQueryEngine.HostingComTr;
using DomainQueryEngine.Infrastructure.Test.Fixtures;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real responses captured from the provider's site (November 2026).

    public class HostingComTrParserTests
    {
        private readonly HostingComTrStrategyParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_With_Try_Prices()
        {
            var results = _parser.ParseDomainResult(RecordedResponse.Read("hostingcomtr-available.json"));
            var result = results.Single(r => r.DomainName == "ornekyazilimci.com");

            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.Equal(Enumerable.Range(1, 10), result.Prices.Select(p => p.Period));
            WhmcsParserTests.AssertPrice(449.50m, "TRY", result.Prices[0].RegistrationPrice);
            WhmcsParserTests.AssertPrice(7645.00m, "TRY", result.Prices[9].RegistrationPrice);
        }

        [Fact]
        public void Lists_Alternative_Extensions_Once_Each()
        {
            var results = _parser.ParseDomainResult(RecordedResponse.Read("hostingcomtr-available.json"));

            Assert.True(results.Count > 1);
            Assert.Equal(results.Count, results.Select(r => r.DomainName).Distinct().Count());
        }

        [Fact]
        public void Parses_Registered_Domain_As_Transferable()
        {
            var results = _parser.ParseDomainResult(RecordedResponse.Read("hostingcomtr-taken.json"));
            var result = results.Single(r => r.DomainName == "hurriyet.com.tr");

            Assert.Equal("com.tr", result.Tld);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Rejects_Error_Response()
        {
            Assert.Throws<ArgumentException>(() => _parser.ParseDomainResult("""{"status":"error","message":"Invalid token"}"""));
        }
    }
}
