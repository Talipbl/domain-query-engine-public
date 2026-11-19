using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DomainQueryEngine.SearchUI.Test
{
    public class PageTests : IClassFixture<SiteFactory>
    {
        private readonly SiteFactory _factory;
        private readonly HttpClient _client;

        public PageTests(SiteFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [Theory]
        [InlineData("/", "Alan adını en ucuz nereden alacağını tek aramada gör")]
        [InlineData("/uzantilar", "Uzantı fiyatları")]
        [InlineData("/uzantilar?grup=tr&sirala=kayit", ".com.tr")]
        [InlineData("/uzantilar/com-tr", ".com.tr fiyatları")]
        [InlineData("/saglayicilar", "Sağlayıcılar")]
        [InlineData("/saglayicilar/turhost", "Turhost")]
        [InlineData("/gizlilik", "Gizlilik")]
        [InlineData("/ara?q=ornek.com", "ornek.com için sağlayıcılara soruluyor")]
        [InlineData("/_bilesenler", "Bileşenler")]
        public async Task Page_Renders(string url, string expectedText)
        {
            var response = await _client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expectedText, html);
            Assert.Contains("<html lang=\"tr\"", html);
        }

        [Theory]
        [InlineData("/uzantilar/bilinmeyen")]
        [InlineData("/saglayicilar/bilinmeyen")]
        [InlineData("/olmayan-sayfa")]
        public async Task Unknown_Pages_Return_The_Not_Found_Page(string url)
        {
            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("Aradığın sayfa bulunamadı", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/uzantilar/.com.tr", HttpStatusCode.MovedPermanently, "/uzantilar/com-tr")]
        [InlineData("/saglayicilar/TURHOST", HttpStatusCode.MovedPermanently, "/saglayicilar/turhost")]
        [InlineData("/?q=ornek.com", HttpStatusCode.Redirect, "/ara?q=ornek.com")]
        [InlineData("/ara", HttpStatusCode.Redirect, "/")]
        public async Task Old_And_Alternative_Addresses_Redirect(string url, HttpStatusCode status, string location)
        {
            var response = await _client.GetAsync(url);

            Assert.Equal(status, response.StatusCode);
            Assert.Equal(location, response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task Invalid_Domain_Shows_An_Error_On_The_Search_Page()
        {
            var html = await _client.GetStringAsync("/ara?q=" + Uri.EscapeDataString("a:b"));

            Assert.Contains("Geçerli bir alan adı yaz", html);
            Assert.DoesNotContain("hx-trigger=\"load\"", html);
        }

        [Fact]
        public async Task Search_Results_List_Providers_And_Count_The_Search_Once()
        {
            var domain = $"sayim-{Guid.NewGuid():N}.com"[..20] + ".com";

            var html = await _client.GetStringAsync($"/ara/sonuclar?q={domain}");
            await _client.GetStringAsync($"/ara/sonuclar?q={domain}&reprice=true&sure=1");
            await _client.GetStringAsync($"/ara/sonuclar?q={domain}&reprice=true&musait=true");

            Assert.Contains("Lira Host", html);
            Assert.Contains("Dolar Host", html);
            Assert.Contains("Kampanya", html);
            Assert.Contains("1 sağlayıcıdan yanıt alınamadı", html);
            Assert.Contains("Sorgu adresi bulunamadı (404)", html);
            Assert.Single(_factory.Searches.Domains, d => d == domain);
        }

        [Fact]
        public async Task Search_Results_Sort_By_Price_In_Lira()
        {
            var html = await _client.GetStringAsync("/ara/sonuclar?q=siralama.com&reprice=true");

            // Dolar Host: 5 USD = 250 TRY, cheaper than Lira Host's 300 TRY.
            Assert.True(html.IndexOf("Dolar Host", StringComparison.Ordinal) < html.IndexOf("Lira Host", StringComparison.Ordinal));
            Assert.Contains("≈", html);
            Assert.Contains("250,00 ₺", html);
        }

        [Fact]
        public async Task Htmx_Results_Request_Updates_The_Address_Bar()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/ara/sonuclar?q=ornek.com&reprice=true&sure=2&musait=true");
            request.Headers.Add("HX-Request", "true");

            var response = await _client.SendAsync(request);

            Assert.Equal("/ara?q=ornek.com&musait=true&sure=2", Assert.Single(response.Headers.GetValues("HX-Replace-Url")));
        }

        [Fact]
        public async Task Saved_Currency_Changes_The_Prices_Shown()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/uzantilar/com-tr");
            request.Headers.Add("Cookie", "dqe_currency=USD");

            var html = await (await _client.SendAsync(request)).Content.ReadAsStringAsync();

            Assert.Contains(" $", html);
            Assert.Contains("aria-pressed=\"true\">$ USD", html);
        }

        [Fact]
        public async Task Currency_Form_Requires_The_Antiforgery_Token()
        {
            var response = await _client.PostAsync("/tercihler/para-birimi", new FormUrlEncodedContent(new Dictionary<string, string> { ["currency"] = "USD" }));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Lookup_Api_Still_Returns_Results()
        {
            var response = await _client.GetAsync("/api/lookup/ornek.com?reprice=true");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Lira Host", await response.Content.ReadAsStringAsync());
        }
    }
}
