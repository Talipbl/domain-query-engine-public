using System.Xml.Linq;

namespace DomainQueryEngine.SearchUI.Test
{
    public class SeoTests : IClassFixture<SiteFactory>
    {
        private readonly HttpClient _client;

        public SeoTests(SiteFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Sitemap_Lists_Catalog_Pages_With_Absolute_Urls()
        {
            var xml = XDocument.Parse(await _client.GetStringAsync("/sitemap.xml"));
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urls = xml.Descendants(ns + "loc").Select(e => e.Value).ToList();

            Assert.Contains("https://test.example/", urls);
            Assert.Contains("https://test.example/uzantilar", urls);
            Assert.Contains("https://test.example/uzantilar/com-tr", urls);
            Assert.Contains("https://test.example/saglayicilar/turhost", urls);
            Assert.Contains("https://test.example/gizlilik", urls);
            Assert.DoesNotContain(urls, u => u.Contains("/ara"));
            Assert.Equal(urls.Count, urls.Distinct().Count());
        }

        [Fact]
        public async Task Robots_Keeps_Search_And_Api_Out_And_Points_To_Sitemap()
        {
            var robots = await _client.GetStringAsync("/robots.txt");

            Assert.Contains("Disallow: /ara", robots);
            Assert.Contains("Disallow: /api/", robots);
            Assert.Contains("Sitemap: https://test.example/sitemap.xml", robots);
        }

        [Fact]
        public async Task Catalog_Pages_Have_A_Canonical_Url_Without_Query()
        {
            var html = await _client.GetStringAsync("/uzantilar?grup=tr");

            Assert.Contains("<link rel=\"canonical\" href=\"https://test.example/uzantilar\" />", html);
            Assert.Contains("<meta property=\"og:url\" content=\"https://test.example/uzantilar\" />", html);
            Assert.DoesNotContain("noindex", html);
        }

        [Fact]
        public async Task Search_Pages_Are_Not_Indexed()
        {
            var html = await _client.GetStringAsync("/ara?q=ornek.com");

            Assert.Contains("<meta name=\"robots\" content=\"noindex\" />", html);
            Assert.DoesNotContain("rel=\"canonical\"", html);
        }

        [Fact]
        public async Task Texts_Come_From_The_Turkish_Locale_By_Default()
        {
            var html = await _client.GetStringAsync("/");

            Assert.Contains(">Uzantı Fiyatları</a>", html);
            Assert.Contains("İçeriğe geç", html);
            Assert.DoesNotContain("Nav.Tlds", html);
        }
    }
}
