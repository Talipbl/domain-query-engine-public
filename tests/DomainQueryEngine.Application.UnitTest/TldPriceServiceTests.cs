using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;

namespace DomainQueryEngine.Application.Test
{
    public class TldPriceServiceTests
    {
        private static readonly ExchangeRates Rates =
            new(new DateTime(2026, 11, 19), "TCMB", new Dictionary<string, decimal> { ["USD"] = 50m, ["EUR"] = 55m });

        private static readonly List<ProviderEntry> Providers = new()
        {
            new() { Slug = "lira", Name = "Lira Host" },
            new() { Slug = "dolar", Name = "Dolar Host" },
            new() { Slug = "pahali", Name = "Pahalı Host" }
        };

        private static readonly List<TldEntry> Tlds = new()
        {
            new()
            {
                Tld = "com",
                Prices =
                {
                    Price("lira", "TRY", registration: 300m, regular: 600m, renewal: 600m),
                    Price("dolar", "USD", registration: 5m, renewal: 10m, registrationTermYears: 2), // 250 TRY, 500
                    Price("pahali", "TRY", registration: 900m)
                }
            },
            new()
            {
                Tld = "com.tr",
                Prices = { Price("lira", "TRY", registration: 50m), Price("dolar", "USD", registration: 1.5m) } // 75 TRY
            },
            new()
            {
                Tld = "xyz",
                Prices = { Price("dolar", "USD", registration: 0.8m) } // 40 TRY
            }
        };

        private static TldPriceService Service(ExchangeRates? rates = null, bool noRates = false)
            => new(new FakeTlds(), new FakeProviders(), new FixedRates(noRates ? null : rates ?? Rates));

        [Fact]
        public async Task Picks_Cheapest_Offers_After_Converting_To_Lira()
        {
            var list = await Service().GetListAsync("TRY");

            var com = list.Items.Single(i => i.Entry.Tld == "com");
            Assert.Equal("Dolar Host", com.CheapestRegistration!.Provider.Name);
            Assert.Equal(250m, com.CheapestRegistration.Display.Amount);
            Assert.True(com.CheapestRegistration.IsConverted);
            Assert.Equal(5m, com.CheapestRegistration.Original.Amount);
            Assert.Equal(500m, com.CheapestRenewal!.Display.Amount);
            Assert.Equal(2, com.CheapestRegistration.TermYears);
            Assert.Equal(3, com.ProviderCount);
        }

        [Fact]
        public async Task Converts_Into_Dollars_When_Selected()
        {
            var list = await Service().GetListAsync("usd");

            var comTr = list.Items.Single(i => i.Entry.Tld == "com.tr");
            Assert.Equal("USD", list.Currency);
            Assert.Equal("Lira Host", comTr.CheapestRegistration!.Provider.Name);
            Assert.Equal(1m, comTr.CheapestRegistration.Display.Amount); // 50 TRY
        }

        [Fact]
        public async Task Lists_Extensions_Alphabetically()
        {
            var list = await Service().GetListAsync("TRY");

            Assert.Equal(new[] { "com", "com.tr", "xyz" }, list.Items.Select(i => i.Entry.Tld));
            Assert.Equal(new[] { "com", "com.tr", "xyz" }, list.Items.Select(i => i.Entry.Tld));
        }

        [Fact]
        public async Task Sorts_By_Price()
        {
            var cheapestFirst = await Service().GetListAsync("TRY", TldSort.CheapestRegistration);
            Assert.Equal(new[] { "xyz", "com.tr", "com" }, cheapestFirst.Items.Select(i => i.Entry.Tld));
        }

        [Fact]
        public async Task Detail_Orders_Providers_And_Marks_The_Cheapest()
        {
            var detail = await Service().GetDetailAsync(".com", "TRY");

            Assert.NotNull(detail);
            Assert.Equal(new[] { "Dolar Host", "Lira Host", "Pahalı Host" }, detail!.Rows.Select(r => r.Provider.Name));
            Assert.True(detail.Rows[0].IsCheapest);
            Assert.False(detail.Rows[1].IsCheapest);
            Assert.Equal(250m, detail.CheapestRegistration!.Display.Amount);
        }

        [Fact]
        public async Task Detail_Shows_Regular_Price_Next_To_Campaign_Price()
        {
            var detail = await Service().GetDetailAsync("com", "TRY");

            var lira = detail!.Rows.Single(r => r.Provider.Slug == "lira").Registration!;
            Assert.True(lira.IsCampaign);
            Assert.Equal(600m, lira.Regular!.Amount);
        }

        [Fact]
        public async Task Detail_Returns_Null_For_Unknown_Extension()
        {
            Assert.Null(await Service().GetDetailAsync("bilinmeyen", "TRY"));
        }

        [Fact]
        public async Task Without_Rates_Compares_Lira_Prices_And_Keeps_Others_Unconverted()
        {
            var detail = await Service(noRates: true).GetDetailAsync("com", "TRY");

            Assert.Null(detail!.Rates);
            Assert.Equal(new[] { "Lira Host", "Pahalı Host", "Dolar Host" }, detail.Rows.Select(r => r.Provider.Name));
            Assert.Equal("USD", detail.Rows[2].Registration!.Display.Currency);
        }

        private static TldProviderPrice Price(string provider, string currency, decimal? registration = null, decimal? regular = null, decimal? renewal = null, int registrationTermYears = 1)
            => new() { Provider = provider, Currency = currency, Registration = registration, RegularRegistration = regular, Renewal = renewal, RegistrationTermYears = registrationTermYears };

        private sealed class FakeTlds : ITldPriceCatalog
        {
            public Task<DateTime> GetAsOfAsync(CancellationToken ct = default) => Task.FromResult(new DateTime(2026, 11, 19));
            public Task<IReadOnlyList<TldEntry>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TldEntry>>(Tlds);
            public Task<TldEntry?> GetAsync(string tld, CancellationToken ct = default)
                => Task.FromResult(Tlds.FirstOrDefault(t => t.Tld == tld.TrimStart('.').Replace('-', '.')));
        }

        private sealed class FakeProviders : IProviderDirectory
        {
            public Task<IReadOnlyList<ProviderEntry>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ProviderEntry>>(Providers);
            public Task<ProviderEntry?> GetAsync(string slug, CancellationToken ct = default) => Task.FromResult(Providers.FirstOrDefault(p => p.Slug == slug));
        }

        private sealed class FixedRates : IExchangeRateProvider
        {
            private readonly ExchangeRates? _rates;
            public FixedRates(ExchangeRates? rates) => _rates = rates;
            public Task<ExchangeRates?> GetRatesAsync(CancellationToken ct = default) => Task.FromResult(_rates);
        }
    }
}
