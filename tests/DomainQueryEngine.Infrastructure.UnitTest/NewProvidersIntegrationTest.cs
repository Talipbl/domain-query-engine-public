using DomainQueryEngine.Infrastructure.DomainSearch.Providers.AtakDomain;
using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.GuzelHosting;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.HostingComTr;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turhost;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turkticaret;

namespace DomainQueryEngine.Infrastructure.Test
{
    /// <summary>
    /// Sends real requests to the providers added in November 2026. Excluded from the default run;
    /// use --filter "Category=Integration".
    /// </summary>
    [Trait("Category", "Integration")]
    public class NewProvidersIntegrationTest
    {
        // Güzel Hosting is left out: Cloudflare blocks server-side requests to it with a bot challenge.
        public static TheoryData<string> Providers => new() { "atak", "turhost", "turkticaret", "hostingcomtr" };

        private static IDomainAvailabilityProvider Create(string provider)
        {
            var http = TestHttpClientFactory.Create();
            return provider switch
            {
                "atak" => new AtakDomainDomainAvailabilityProvider(new AtakDomainLookupResponseParser(), http),
                "turhost" => new TurhostDomainAvailabilityProvider(new TurhostLookupResponseParser(), http),
                "guzel" => new GuzelHostingDomainAvailabilityProvider(new GuzelHostingLookupResponseParser(), http),
                "turkticaret" => new TurkticaretDomainAvailabilityProvider(new TurkticaretLookupResponseParser(), http),
                "hostingcomtr" => new HostingComTrDomainAvailabilityProvider(new HostingComTrLookupResponseParser(), http),
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };
        }

        [Theory]
        [MemberData(nameof(Providers))]
        public async Task Finds_Unregistered_Domain_With_A_Price(string provider)
        {
            var domain = $"dqe-test-{Guid.NewGuid():N}".Substring(0, 24) + ".com";

            var result = await Create(provider).LookupAsync(domain);

            Assert.Equal(domain, result.DomainName);
            Assert.True(result.IsAvailable);
            Assert.Contains(result.Prices, p => p.RegistrationPrice is { Amount: > 0 });
        }

        [Theory]
        [MemberData(nameof(Providers))]
        public async Task Reports_Registered_Domain_As_Unavailable(string provider)
        {
            var result = await Create(provider).LookupAsync("hurriyet.com.tr");

            Assert.Equal("hurriyet.com.tr", result.DomainName);
            Assert.False(result.IsAvailable);
        }
    }
}
