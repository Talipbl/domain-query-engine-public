using DomainQueryEngine.Infrastructure.DomainSearch.Providers.AAK;
using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.GriNet;
using DomainQueryEngine.Infrastructure.Test.Fixtures;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.IsimKayit;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Natro;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YildizBilisim;

namespace DomainQueryEngine.Infrastructure.Test.Parsers
{
    /// <summary>
    /// Yildiz Bilisim, GriNet and Alan Adi Kayit all use the WHMCS domain check API and share one format.
    /// </summary>
    public class WhmcsParserTests
    {
        public static TheoryData<IDomainLookupResponseParser> Parsers => new()
        {
            new YildizBilisimLookupResponseParser(),
            new GriNetLookupResponseParser(),
            new AlanAdiKayitLookupResponseParser()
        };

        [Theory]
        [MemberData(nameof(Parsers))]
        public void Parses_Available_Domain_With_All_Periods(IDomainLookupResponseParser parser)
        {
            var result = Assert.Single(parser.ParseDomainResult(SampleResponses.WhmcsAvailable));

            Assert.Equal("salihotoyikama.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.True(result.IsValidDomain);
            Assert.False(result.IsPremium);
            Assert.Equal(new[] { 1, 5, 10 }, result.Prices.Select(p => p.Period).OrderBy(p => p));

            var oneYear = result.Prices.Single(p => p.Period == 1);
            AssertPrice(4000m, "TRY", oneYear.RegistrationPrice);
            AssertPrice(4000m, "TRY", oneYear.TransferPrice);
            AssertPrice(4000m, "TRY", oneYear.RenewalPrice);

            var tenYears = result.Prices.Single(p => p.Period == 10);
            AssertPrice(27000m, "TRY", tenYears.RegistrationPrice);
            Assert.Equal(0m, tenYears.RenewalPrice.Amount); // not offered for 10 years
        }

        [Theory]
        [MemberData(nameof(Parsers))]
        public void Parses_Taken_Domain_Without_Prices(IDomainLookupResponseParser parser)
        {
            var result = Assert.Single(parser.ParseDomainResult(SampleResponses.WhmcsTaken));

            Assert.Equal("google.com", result.DomainName);
            Assert.False(result.IsAvailable);
            Assert.Empty(result.Prices);
        }

        [Theory]
        [MemberData(nameof(Parsers))]
        public void Rejects_Response_Without_Result(IDomainLookupResponseParser parser)
        {
            Assert.Throws<ArgumentException>(() => parser.ParseDomainResult("{}"));
        }

        internal static void AssertPrice(decimal amount, string currency, BasePrice? price)
        {
            Assert.NotNull(price);
            Assert.Equal(amount, price!.Amount);
            Assert.Equal(currency, price.Currency);
        }
    }

    public class IsimKayitParserTests
    {
        private readonly IsimKayitLookupResponseParser _parser = new();

        [Fact]
        public void Parses_Available_Domain()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.WhmcsAvailable));

            Assert.True(result.IsAvailable);
            Assert.Equal(new[] { 1, 5, 10 }, result.Prices.Select(p => p.Period).OrderBy(p => p));
            WhmcsParserTests.AssertPrice(4000m, "TRY", result.Prices.Single(p => p.Period == 1).RenewalPrice);
        }

        [Fact]
        public void Adds_Shortest_Period_When_Missing_From_Pricing()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.WhmcsShortestPeriodOnly));

            Assert.Equal(new[] { 1, 2 }, result.Prices.Select(p => p.Period).OrderBy(p => p));
            var oneYear = result.Prices.Single(p => p.Period == 1);
            WhmcsParserTests.AssertPrice(500m, "TRY", oneYear.RegistrationPrice);
            WhmcsParserTests.AssertPrice(550m, "TRY", oneYear.RenewalPrice);
        }

        [Fact]
        public void Leaves_Missing_Prices_Null()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.WhmcsShortestPeriodOnly));

            Assert.Null(result.Prices.Single(p => p.Period == 2).TransferPrice);
        }
    }

    public class NatroParserTests
    {
        private readonly NatroLookupResponseParser _parser = new();

        [Fact]
        public void Parses_Available_Domain_With_Usd_Price()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.NatroAvailable));

            Assert.Equal("ornek.com", result.DomainName);
            Assert.Equal("com", result.Tld);
            Assert.True(result.IsAvailable);
            Assert.True(result.IsValidDomain);

            var price = Assert.Single(result.Prices);
            Assert.Equal(1, price.Period);
            WhmcsParserTests.AssertPrice(9.99m, "USD", price.RegistrationPrice);
        }

        [Fact]
        public void Parses_Taken_Domain_Without_Prices()
        {
            var result = Assert.Single(_parser.ParseDomainResult(SampleResponses.NatroTaken));

            Assert.False(result.IsAvailable);
            Assert.Empty(result.Prices);
        }
    }
}
