using DomainQueryEngine.AAK;
using DomainQueryEngine.GriNet;
using DomainQueryEngine.YildizBilisim;
using Moq;

namespace DomainQueryEngine.Infrastructure.Test
{
    [Trait("Category", "Integration")]
    public class GriNetLookupStrategyIntegrationTest
    {
        GriNetLookupStrategy _lookupStrategy;
        GriNetStrategyParser _parser;
        public GriNetLookupStrategyIntegrationTest()
        {
            _parser = new GriNetStrategyParser();
            _lookupStrategy = new GriNetLookupStrategy(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Return_Parsed_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }

    [Trait("Category", "Integration")]
    public class AlanAdiKayitLookupStrategyIntegrationTest
    {
        AlanAdiKayitLookupStrategy _lookupStrategy;
        AlanAdiKayitStrategyParser _parser;
        public AlanAdiKayitLookupStrategyIntegrationTest()
        {
            _parser = new AlanAdiKayitStrategyParser();
            _lookupStrategy = new AlanAdiKayitLookupStrategy(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Return_Parsed_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}