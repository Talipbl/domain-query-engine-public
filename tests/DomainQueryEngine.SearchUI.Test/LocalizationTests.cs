using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DomainQueryEngine.SearchUI.Test
{
    /// <summary>Language and theme preferences, and the key/value locale files behind the texts.</summary>
    public class LocalizationTests : IClassFixture<SiteFactory>
    {
        private const string EnglishCookie = ".AspNetCore.Culture=c%3Den-US%7Cuic%3Den-US";
        private readonly HttpClient _client;

        public LocalizationTests(SiteFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        private async Task<string> GetWithCookie(string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);
            return await (await _client.SendAsync(request)).Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task Saved_English_Choice_Shows_The_Pages_In_English()
        {
            var html = await GetWithCookie("/", EnglishCookie);

            Assert.Contains("<html lang=\"en\"", html);
            Assert.Contains("See where to get your domain cheapest in one search", html);
            Assert.Contains(">Extension Prices</a>", html);
            Assert.Matches(@"value=""en""[^>]*aria-pressed=""true""", html);
        }

        [Fact]
        public async Task English_Formats_Prices_The_English_Way()
        {
            var html = await GetWithCookie("/ara/sonuclar?q=dil.com&reprice=true", EnglishCookie);

            Assert.Contains("₺250.00", html);
            Assert.Contains("Asked 3 providers", html);
            Assert.Contains("1 provider did not respond", html); // singular ".One" form
            Assert.Contains("Query address not found (404)", html);
        }

        [Fact]
        public async Task Culture_Can_Be_Picked_In_The_Query_String()
        {
            var html = await _client.GetStringAsync("/uzantilar?culture=en-US&ui-culture=en-US");

            Assert.Contains("Extension prices", html);
        }

        [Fact]
        public async Task Saved_Theme_Is_Rendered_On_The_Page()
        {
            Assert.Contains("data-theme=\"dark\"", await GetWithCookie("/", "dqe_theme=dark"));
            // No valid choice: the page follows the system setting (Razor always writes data-* attributes).
            Assert.Contains("data-theme=\"\"", await GetWithCookie("/", "dqe_theme=pink"));
        }

        [Fact]
        public async Task Home_Lists_Extensions_Not_Domains()
        {
            var html = await _client.GetStringAsync("/");
            var section = html[html.IndexOf("Son 24 saatte en çok sorgulanan uzantılar", StringComparison.Ordinal)..];
            section = section[..section.IndexOf("</ol>", StringComparison.Ordinal)];

            Assert.Contains("href=\"/uzantilar/com-tr\">.com.tr</a>", section);
            Assert.Contains("sorgu", section);
            Assert.DoesNotContain("kahveci", section);
        }

        [Theory]
        [InlineData("/tercihler/tema", "theme", "dark")]
        [InlineData("/tercihler/dil", "culture", "en")]
        public async Task Preference_Forms_Require_The_Antiforgery_Token(string url, string field, string value)
        {
            var response = await _client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string> { [field] = value }));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static Dictionary<string, string> ReadLocale(string language)
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream($"Locales.{language}.json")
                ?? throw new InvalidOperationException($"Locales.{language}.json is not embedded.");
            return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
        }

        [Fact]
        public void Turkish_And_English_Have_The_Same_Keys()
        {
            var tr = ReadLocale("tr");
            var en = ReadLocale("en");

            Assert.Empty(tr.Keys.Except(en.Keys));
            Assert.Empty(en.Keys.Except(tr.Keys));
            Assert.All(tr.Concat(en), pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value), pair.Key));
        }

        [Fact]
        public void Every_Key_Used_In_The_Code_Exists()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "DomainQueryEngine.sln")))
                root = root.Parent;
            Assert.NotNull(root);

            var project = Path.Combine(root!.FullName, "src", "DomainQueryEngine.SearchUI");
            var keyPattern = new Regex(@"T\[""(?<key>[A-Z][A-Za-z0-9]*\.[A-Za-z0-9.]+)""|""(?<key>(?:Error|Search|Tlds\.Group|Tlds\.Sort)\.[A-Za-z0-9.]+)""");
            var used = Directory.EnumerateFiles(project, "*.*", SearchOption.AllDirectories)
                .Where(f => (f.EndsWith(".cshtml") || f.EndsWith(".cs")) && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .SelectMany(f => keyPattern.Matches(File.ReadAllText(f)).Select(m => m.Groups["key"].Value))
                .ToHashSet();

            var tr = ReadLocale("tr");
            Assert.NotEmpty(used);
            Assert.Empty(used.Where(k => !tr.ContainsKey(k)));
        }
    }
}
