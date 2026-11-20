namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.YildizBilisim
{
    [Trait("Category", "Integration")]
    public class YildizBilisimDomainAvailabilityProviderIntegrationTest
    {
        YildizBilisimDomainAvailabilityProvider _lookupStrategy;
        YildizBilisimLookupResponseParser _parser;
        public YildizBilisimDomainAvailabilityProviderIntegrationTest()
        {
            _parser = new YildizBilisimLookupResponseParser();
            _lookupStrategy = new YildizBilisimDomainAvailabilityProvider(_parser, TestHttpClientFactory.Create());
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }
    }
}