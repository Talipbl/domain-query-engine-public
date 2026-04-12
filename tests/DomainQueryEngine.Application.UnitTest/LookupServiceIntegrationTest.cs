using System.Net.Http;
using System.Threading.Tasks;

namespace DomainQueryEngine.Application.Test
{
    public class LookupServiceIntegrationTest : TestBase
    {
        [Fact]
        public async Task Should_Lookup_Domain_Async()
        {
            var result = await LookupService.LookupDomainAsync("ornekyazilimci.com");
        }
    }
}