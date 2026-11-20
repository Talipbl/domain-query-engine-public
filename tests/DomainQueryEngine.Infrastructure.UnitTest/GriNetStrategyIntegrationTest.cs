using DomainQueryEngine.Infrastructure.DomainSearch.Providers.AAK;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.GriNet;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YildizBilisim;
using Moq;

namespace DomainQueryEngine.Infrastructure.Test
{
    [Trait("Category", "Integration")]
    public class GriNetDomainAvailabilityProviderIntegrationTest
    {
        GriNetDomainAvailabilityProvider _lookupStrategy;
        GriNetLookupResponseParser _parser;
        public GriNetDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new GriNetLookupResponseParser();
            _lookupStrategy = new GriNetDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Return_Parsed_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }

    [Trait("Category", "Integration")]
    public class AlanAdiKayitDomainAvailabilityProviderIntegrationTest
    {
        AlanAdiKayitDomainAvailabilityProvider _lookupStrategy;
        AlanAdiKayitLookupResponseParser _parser;
        public AlanAdiKayitDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new AlanAdiKayitLookupResponseParser();
            _lookupStrategy = new AlanAdiKayitDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Return_Parsed_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}