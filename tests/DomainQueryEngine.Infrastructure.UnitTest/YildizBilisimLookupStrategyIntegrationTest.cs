namespace DomainQueryEngine.YildizBilisim
{
    [Trait("Category", "Integration")]
    public class YildizBilisimLookupStrategyIntegrationTest
    {
        YildizBilisimLookupStrategy _lookupStrategy;
        YildizBilisimStrategyParser _parser;
        public YildizBilisimLookupStrategyIntegrationTest()
        {
            _parser = new YildizBilisimStrategyParser();
            _lookupStrategy = new YildizBilisimLookupStrategy(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}