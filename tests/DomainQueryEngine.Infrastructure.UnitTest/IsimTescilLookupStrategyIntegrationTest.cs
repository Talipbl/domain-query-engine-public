using DomainQueryEngine.Infrastructure.DomainSearch.Providers.IsimTescil;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.Natro
{
    [Trait("Category", "Integration")]
    public class IsimTescilDomainAvailabilityProviderIntegrationTest
    {
        IsimTescilDomainAvailabilityProvider _lookupStrategy;
        IsimTescilLookupResponseParser _parser;
        public IsimTescilDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new IsimTescilLookupResponseParser();
            _lookupStrategy = new IsimTescilDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}