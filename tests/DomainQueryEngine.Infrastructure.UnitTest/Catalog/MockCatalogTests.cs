using DomainQueryEngine.Core.Catalog;
using DomainQueryEngine.Infrastructure.Catalog;

namespace DomainQueryEngine.Infrastructure.Test.Catalog
{
    public class MockTldCatalogTests
    {
        private readonly MockTldCatalog _catalog = new();

        [Fact]
        public async Task Loads_Every_Extension_With_Prices()
        {
            var all = await _catalog.GetAllAsync();

            Assert.True(_catalog.IsSample);
            Assert.Equal(new DateTime(2026, 11, 19), _catalog.AsOf);
            Assert.Equal(15, all.Count);
            Assert.Equal(all.Count, all.Select(t => t.Tld).Distinct().Count());
            Assert.All(all, t =>
            {
                Assert.NotEmpty(t.Prices);
                Assert.Contains(t.Prices, p => p.Registration > 0);
                Assert.Equal(6, t.History.Count);
            });
        }

        [Theory]
        [InlineData("com.tr")]
        [InlineData(".com.tr")]
        [InlineData("COM.TR")]
        [InlineData("com-tr")]
        public async Task Finds_Extension_By_Name_Or_Slug(string input)
        {
            var entry = await _catalog.GetAsync(input);

            Assert.NotNull(entry);
            Assert.Equal("com.tr", entry!.Tld);
            Assert.Equal("com-tr", entry.Slug);
            Assert.Equal(TldCategory.Turkish, entry.Category);
            Assert.False(entry.DocumentRequired);
        }

        [Fact]
        public async Task Returns_Null_For_Unknown_Extension()
        {
            Assert.Null(await _catalog.GetAsync("bilinmeyen"));
        }

        [Fact]
        public async Task Keeps_Real_Com_Prices_From_Live_Lookups()
        {
            var com = (await _catalog.GetAsync("com"))!;

            var turkticaret = com.Prices.Single(p => p.Provider == "turkticaret");
            Assert.Equal(128.99m, turkticaret.Registration);
            Assert.Equal(682.99m, turkticaret.RegularRegistration);
            Assert.True(turkticaret.IsCampaign);

            var natro = com.Prices.Single(p => p.Provider == "natro");
            Assert.Equal("USD", natro.Currency);
            Assert.Equal(2.99m, natro.Registration);
            Assert.False(natro.IsCampaign);
        }

        [Fact]
        public async Task Every_Price_Refers_To_A_Known_Provider()
        {
            var providers = (await new MockProviderCatalog().GetAllAsync()).Select(p => p.Slug).ToHashSet();
            var tlds = await _catalog.GetAllAsync();

            Assert.All(tlds.SelectMany(t => t.Prices), p => Assert.Contains(p.Provider, providers));
        }
    }

    public class MockProviderCatalogTests
    {
        private readonly MockProviderCatalog _catalog = new();

        [Fact]
        public async Task Lists_Providers_With_Their_Status()
        {
            var all = await _catalog.GetAllAsync();

            Assert.Equal(13, all.Count);
            Assert.Equal(9, all.Count(p => p.Status == ProviderStatus.Active));
            Assert.Equal(ProviderStatus.Failing, all.Single(p => p.Slug == "yoncu-bilisim").Status);
            Assert.Equal(ProviderStatus.Disabled, all.Single(p => p.Slug == "guzel-hosting").Status);
        }

        [Fact]
        public async Task Finds_Provider_By_Slug()
        {
            var provider = await _catalog.GetAsync("Turhost");

            Assert.NotNull(provider);
            Assert.Equal("Turhost", provider!.Name);
            Assert.Equal("https://www.turhost.com", provider.Website);
        }
    }

    public class MockSearchStatisticsTests
    {
        [Fact]
        public async Task Returns_Most_Searched_First()
        {
            var stats = new MockSearchStatistics();

            var top = await stats.GetMostSearchedAsync(TimeSpan.FromDays(1), 3);

            Assert.True(stats.IsSample);
            Assert.Equal(3, top.Count);
            Assert.Equal("kahveci.com.tr", top[0].DomainName);
            Assert.True(top[0].Count >= top[1].Count && top[1].Count >= top[2].Count);
        }

        [Fact]
        public async Task Groups_Searches_By_Extension()
        {
            var stats = new MockSearchStatistics();

            var top = await stats.GetMostSearchedTldsAsync(TimeSpan.FromDays(1), 5);

            Assert.NotEmpty(top);
            Assert.All(top, t => Assert.False(t.Tld.StartsWith('.')));
            Assert.Equal(top.Count, top.Select(t => t.Tld).Distinct().Count());
            Assert.True(top.Zip(top.Skip(1)).All(p => p.First.Count >= p.Second.Count));
            Assert.Contains(top, t => t.Tld == "com.tr" && t.Slug == "com-tr");
        }
    }
}
