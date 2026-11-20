using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YoncuBilisim;
using Moq;

namespace DomainQueryEngine.Infrastructure.Test
{
    [Trait("Category", "Integration")]
    public class YoncuBilisimDomainAvailabilityProviderIntegrationTest
    {
        YoncuBilisimDomainAvailabilityProvider _lookupStrategy;
        YoncuBilisimLookupResponseParser _parser;
        public YoncuBilisimDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new YoncuBilisimLookupResponseParser();
            _lookupStrategy = new YoncuBilisimDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}
