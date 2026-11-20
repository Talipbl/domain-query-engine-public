namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.Natro
{
    [Trait("Category", "Integration")]
    public class NatroDomainAvailabilityProviderIntegrationTest
    {
        NatroDomainAvailabilityProvider _lookupStrategy;
        NatroLookupResponseParser _parser;
        public NatroDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new NatroLookupResponseParser();
            _lookupStrategy = new NatroDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}