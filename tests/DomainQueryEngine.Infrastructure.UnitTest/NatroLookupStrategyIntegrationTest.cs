namespace DomainQueryEngine.Natro
{
    public class NatroLookupStrategyIntegrationTest
    {
        NatroLookupStrategy _lookupStrategy;
        NatroStrategyParser _parser;
        public NatroLookupStrategyIntegrationTest()
        {
            _parser = new NatroStrategyParser();
            _lookupStrategy = new NatroLookupStrategy(_parser);
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}