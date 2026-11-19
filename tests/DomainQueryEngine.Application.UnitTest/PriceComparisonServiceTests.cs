using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;

namespace DomainQueryEngine.Application.Test
{
    public class PriceComparisonServiceTests
    {
        private static readonly ExchangeRates Rates =
            new(new DateTime(2026, 11, 19), "TCMB", new Dictionary<string, decimal> { ["USD"] = 50m, ["EUR"] = 55m });

        [Fact]
        public async Task Sorts_By_Price_In_Selected_Currency()
        {
            var service = new PriceComparisonService(new FixedRates(Rates));

            var comparison = await service.CompareAsync(new[]
            {
                Result("try-400", available: true, 400m, "TRY"),
                Result("usd-3", available: true, 3m, "USD"),     // 150 TRY
                Result("usd-9", available: true, 9m, "USD"),     // 450 TRY
                Result("taken", available: false, 1m, "TRY"),
                Result("no-price", available: true)
            }, "TRY");

            Assert.Equal("TRY", comparison.Currency);
            Assert.Equal(new[] { "usd-3", "try-400", "usd-9", "no-price", "taken" }, comparison.Items.Select(i => i.Result.ProviderInfo.Name));

            var cheapest = comparison.Items[0];
            Assert.True(cheapest.IsConverted);
            Assert.Equal(150m, cheapest.DisplayPrice!.Amount);
            Assert.Equal("TRY", cheapest.DisplayPrice.Currency);
            Assert.Equal(3m, cheapest.OriginalPrice!.Amount);
            Assert.Equal("USD", cheapest.OriginalPrice.Currency);
        }

        [Fact]
        public async Task Converts_Lira_Prices_To_Dollars()
        {
            var service = new PriceComparisonService(new FixedRates(Rates));

            var comparison = await service.CompareAsync(new[] { Result("try-500", available: true, 500m, "TRY") }, "usd");

            var item = Assert.Single(comparison.Items);
            Assert.Equal("USD", comparison.Currency);
            Assert.Equal(10m, item.DisplayPrice!.Amount);
            Assert.Equal("USD", item.DisplayPrice.Currency);
        }

        [Fact]
        public async Task Keeps_Original_Prices_And_Order_Without_Rates()
        {
            var service = new PriceComparisonService(new FixedRates(null));

            var comparison = await service.CompareAsync(new[]
            {
                Result("try-400", available: true, 400m, "TRY"),
                Result("usd-3", available: true, 3m, "USD")
            }, "TRY");

            Assert.Null(comparison.Rates);
            Assert.Equal(new[] { "try-400", "usd-3" }, comparison.Items.Select(i => i.Result.ProviderInfo.Name));
            Assert.All(comparison.Items, i => Assert.False(i.IsConverted));
            Assert.Equal("USD", comparison.Items[1].DisplayPrice!.Currency);
        }

        [Fact]
        public async Task Shows_Transfer_Price_For_Registered_Domains()
        {
            var service = new PriceComparisonService(new FixedRates(Rates));
            var registered = new SearchResult
            {
                ProviderInfo = new ProviderInfo { Name = "registered" },
                Domain = new DomainResult
                {
                    DomainName = "ornek.com", IsAvailable = false, IsTransferable = true,
                    Prices = { new DomainPrice { Period = 1, TransferPrice = new BasePrice { Amount = 2m, Currency = "USD" } } }
                }
            };

            var item = Assert.Single((await service.CompareAsync(new[] { registered }, "TRY")).Items);

            Assert.Equal(100m, item.DisplayPrice!.Amount);
        }

        [Fact]
        public async Task Converts_Regular_Price_Of_A_Campaign()
        {
            var service = new PriceComparisonService(new FixedRates(Rates));
            var campaign = Result("kampanya", available: true, 3m, "USD");
            campaign.Domain.Prices[0].RegularRegistrationPrice = new BasePrice { Amount = 9m, Currency = "USD" };

            var item = Assert.Single((await service.CompareAsync(new[] { campaign }, "TRY")).Items);

            Assert.True(item.IsCampaign);
            Assert.Equal(150m, item.DisplayPrice!.Amount);
            Assert.Equal(450m, item.RegularPrice!.Amount);
            Assert.Equal("TRY", item.RegularPrice.Currency);
        }

        [Fact]
        public async Task Compares_The_Requested_Period()
        {
            var service = new PriceComparisonService(new FixedRates(Rates));
            var multiYear = Result("cok-yil", available: true, 100m, "TRY");
            multiYear.Domain.Prices.Add(new DomainPrice { Period = 2, RegistrationPrice = new BasePrice { Amount = 150m, Currency = "TRY" } });
            var oneYearOnly = Result("tek-yil", available: true, 80m, "TRY");

            var comparison = await service.CompareAsync(new[] { oneYearOnly, multiYear }, "TRY", period: 2);

            var first = comparison.Items[0];
            Assert.Equal("cok-yil", first.Result.ProviderInfo.Name);
            Assert.Equal(2, first.Period);
            Assert.Equal(150m, first.DisplayPrice!.Amount);

            var missing = comparison.Items[1];
            Assert.Null(missing.DisplayPrice);
            Assert.Equal(2, missing.Period);
        }

        [Theory]
        [InlineData("usd", "USD")]
        [InlineData(" EUR ", "EUR")]
        [InlineData("GBP", "TRY")]
        [InlineData(null, "TRY")]
        public void Falls_Back_To_Lira_For_Unsupported_Currencies(string? input, string expected)
        {
            Assert.Equal(expected, PriceComparisonService.NormalizeCurrency(input));
        }

        private static SearchResult Result(string name, bool available, decimal? amount = null, string? currency = null)
        {
            var domain = new DomainResult { DomainName = "ornek.com", IsAvailable = available };
            if (amount.HasValue)
                domain.Prices.Add(new DomainPrice { Period = 1, RegistrationPrice = new BasePrice { Amount = amount.Value, Currency = currency! } });
            return new SearchResult { ProviderInfo = new ProviderInfo { Name = name }, Domain = domain };
        }

        private sealed class FixedRates : IExchangeRateProvider
        {
            private readonly ExchangeRates? _rates;
            public FixedRates(ExchangeRates? rates) => _rates = rates;
            public Task<ExchangeRates?> GetRatesAsync(CancellationToken ct = default) => Task.FromResult(_rates);
        }
    }
}
