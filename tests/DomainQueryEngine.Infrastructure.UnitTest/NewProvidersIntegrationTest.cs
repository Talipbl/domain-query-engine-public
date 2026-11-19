using DomainQueryEngine.AtakDomain;
using DomainQueryEngine.Core;
using DomainQueryEngine.GuzelHosting;
using DomainQueryEngine.HostingComTr;
using DomainQueryEngine.Turhost;
using DomainQueryEngine.Turkticaret;

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

        private static IDomainLookupStrategy Create(string provider)
        {
            var http = TestHttpClientFactory.Create();
            return provider switch
            {
                "atak" => new AtakDomainLookupStrategy(new AtakDomainStrategyParser(), http),
                "turhost" => new TurhostLookupStrategy(new TurhostStrategyParser(), http),
                "guzel" => new GuzelHostingLookupStrategy(new GuzelHostingStrategyParser(), http),
                "turkticaret" => new TurkticaretLookupStrategy(new TurkticaretStrategyParser(), http),
                "hostingcomtr" => new HostingComTrLookupStrategy(new HostingComTrStrategyParser(), http),
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
