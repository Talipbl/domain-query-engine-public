namespace DomainQueryEngine.YildizBilisim
{
    public class YildizBilisimLookupStrategyIntegrationTest
    {
        YildizBilisimLookupStrategy _lookupStrategy;
        YildizBilisimStrategyParser _parser;
        public YildizBilisimLookupStrategyIntegrationTest()
        {
            _parser = new YildizBilisimStrategyParser();
            _lookupStrategy = new YildizBilisimLookupStrategy(_parser);
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Parse_Domain_Result()
        {
            string lookupResponse = "{\"result\":[{\"domainName\":\"salihotoyikama.com\",\"idnDomainName\":\"salihotoyikama.com\",\"tld\":\"com\",\"tldNoDots\":\"com\",\"sld\":\"salihotoyikama\",\"idnSld\":\"salihotoyikama\",\"status\":\"available for registration\",\"legacyStatus\":\"available\",\"score\":1,\"isRegistered\":false,\"isAvailable\":true,\"isValidDomain\":true,\"domainErrorMessage\":\"\",\"pricing\":{\"1\":{\"register\":\"\\u20ba4000.00 TL\",\"transfer\":\"\\u20ba4000.00 TL\",\"renew\":\"\\u20ba4000.00 TL\"},\"5\":{\"register\":\"\\u20ba18000.00 TL\",\"renew\":\"\\u20ba18000.00 TL\"},\"10\":{\"register\":\"\\u20ba27000.00 TL\"}},\"shortestPeriod\":{\"period\":1,\"register\":\"\\u20ba4000.00 TL\",\"transfer\":\"\\u20ba4000.00 TL\",\"renew\":\"\\u20ba4000.00 TL\"},\"group\":\"hot\",\"minLength\":3,\"maxLength\":63,\"isPremium\":false,\"premiumCostPricing\":[]}]}";

            var parseResult = _parser.ParseDomainResult(lookupResponse);
        }
    }
}