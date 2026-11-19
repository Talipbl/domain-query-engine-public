using DomainQueryEngine.YoncuBilisim;
using Moq;

namespace DomainQueryEngine.Infrastructure.Test
{
    [Trait("Category", "Integration")]
    public class YoncuBilisimLookupStrategyIntegrationTest
    {
        YoncuBilisimLookupStrategy _lookupStrategy;
        YoncuBilisimStrategyParser _parser;
        public YoncuBilisimLookupStrategyIntegrationTest()
        {
            _parser = new YoncuBilisimStrategyParser();
            _lookupStrategy = new YoncuBilisimLookupStrategy(_parser, TestHttpClientFactory.Create()); 
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}