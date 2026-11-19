using DomainQueryEngine.IsimTescil;

namespace DomainQueryEngine.Natro
{
    [Trait("Category", "Integration")]
    public class IsimTescilLookupStrategyIntegrationTest
    {
        IsimTescilLookupStrategy _lookupStrategy;
        IsimTescilStrategyParser _parser;
        public IsimTescilLookupStrategyIntegrationTest()
        {
            _parser = new IsimTescilStrategyParser();
            _lookupStrategy = new IsimTescilLookupStrategy(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}