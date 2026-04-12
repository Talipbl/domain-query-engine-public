using DomainQueryEngine.IsimTescil;

namespace DomainQueryEngine.Natro
{
    public class IsimTescilLookupStrategyIntegrationTest
    {
        IsimTescilLookupStrategy _lookupStrategy;
        IsimTescilStrategyParser _parser;
        public IsimTescilLookupStrategyIntegrationTest()
        {
            _parser = new IsimTescilStrategyParser();
            _lookupStrategy = new IsimTescilLookupStrategy(_parser);
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}