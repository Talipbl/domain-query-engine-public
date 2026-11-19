using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.YoncuBilisim;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    // Fixtures are real "/API/Domain/Check" responses captured from yoncu.com (November 2026).
    public class YoncuBilisimParserTests
    {
        // The fixtures' campaign_expiry is 1709194765 (2024-02-29).
        private static readonly DateTimeOffset AfterCampaign = new(2026, 11, 19, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset DuringCampaign = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly YoncuBilisimStrategyParser _parser = new() { Now = () => AfterCampaign };

        [Fact]
        public void Parses_Available_Domain()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("yoncu-available.json")));

            Assert.Equal("salihotoyikama.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.True(result.IsValidDomain);
            Assert.False(result.IsTransferable);

            var price = Assert.Single(result.Prices);
            Assert.Equal(1, price.Period);
            WhmcsParserTests.AssertPrice(850m, "TRY", price.RegistrationPrice);
            WhmcsParserTests.AssertPrice(980m, "TRY", price.RenewalPrice);
            WhmcsParserTests.AssertPrice(850m, "TRY", price.TransferPrice);
            Assert.Null(price.RegularRegistrationPrice);
        }

        [Fact]
        public void Ignores_Expired_Campaign_And_Uses_It_While_Active()
        {
            var json = RecordedResponse.Read("yoncu-available.json").Replace("\"campaign_register\":850", "\"campaign_register\":700");

            var expired = Assert.Single(_parser.ParseDomainResult(json)).Prices.Single();
            WhmcsParserTests.AssertPrice(850m, "TRY", expired.RegistrationPrice);
            Assert.Null(expired.RegularRegistrationPrice);

            var activeParser = new YoncuBilisimStrategyParser { Now = () => DuringCampaign };
            var active = Assert.Single(activeParser.ParseDomainResult(json)).Prices.Single();
            WhmcsParserTests.AssertPrice(700m, "TRY", active.RegistrationPrice);
            WhmcsParserTests.AssertPrice(850m, "TRY", active.RegularRegistrationPrice);
        }

        [Fact]
        public void Parses_Taken_Domain_Without_Prices()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("yoncu-taken.json")));

            Assert.Equal("google.com", result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.True(result.IsValidDomain);
            Assert.False(result.IsTransferable);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Parses_Forbidden_Domain_With_Empty_Tld_Info()
        {
            var result = Assert.Single(_parser.ParseDomainResult(RecordedResponse.Read("yoncu-forbidden.json")));

            Assert.Equal("yoncu.com", result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.False(result.IsValidDomain);
            Assert.Empty(result.Prices);
        }

        [Fact]
        public void Throws_When_Api_Reports_Failure()
        {
            Assert.Throws<InvalidOperationException>(() => _parser.ParseDomainResult("[false,\"Token hatası\"]"));
        }

        [Fact]
        public void Reads_Cf_Token_From_Page()
        {
            Assert.Equal("abc123", YoncuBilisimStrategyParser.GetCfToken("<script>window.cftoken = \"abc123\";</script>"));
            Assert.Equal(string.Empty, YoncuBilisimStrategyParser.GetCfToken("<html></html>"));
        }
    }
}
