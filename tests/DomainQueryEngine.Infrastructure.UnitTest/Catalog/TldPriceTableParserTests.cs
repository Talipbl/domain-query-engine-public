using DomainQueryEngine.Infrastructure.TldPricing;

namespace DomainQueryEngine.Infrastructure.Test.Catalog
{
    public sealed class TldPriceTableParserTests
    {
        private readonly TldPriceTableParser _parser = new();

        [Fact]
        public void Parses_Campaign_Prices_And_Minimum_Terms()
        {
            const string html = """
                <table>
                  <thead><tr><th>Uzantı</th><th>Yeni Kayıt (Yıl)</th><th>Transfer</th><th>Yenileme (Yıl)</th></tr></thead>
                  <tbody><tr><td>.com</td><td><del>$22.99</del><span>$2.99 / 2 yıl</span></td><td>$9.99</td><td>$15.99 / 1 yıl</td></tr></tbody>
                </table>
                """;

            var price = Assert.Single(_parser.Parse(html, "natro"));

            Assert.Equal("com", price.Tld);
            Assert.Equal("USD", price.Currency);
            Assert.Equal(2.99m, price.Registration);
            Assert.Equal(22.99m, price.RegularRegistration);
            Assert.Equal(2, price.RegistrationTermYears);
            Assert.Equal(15.99m, price.Renewal);
            Assert.Equal(1, price.RenewalTermYears);
        }

        [Fact]
        public void Uses_The_Provider_Currency_When_A_Cell_Shows_Try_And_Usd()
        {
            const string html = """
                <table>
                  <tr><th>Domain</th><th>Tescil</th><th>Yenileme</th></tr>
                  <tr><td>.com</td><td>1.008,88 TL $19.99</td><td>1.160,29 TL $22.99</td></tr>
                </table>
                """;

            var price = Assert.Single(_parser.Parse(html, "natro"));

            Assert.Equal("USD", price.Currency);
            Assert.Equal(19.99m, price.Registration);
            Assert.Equal(22.99m, price.Renewal);
        }

        [Fact]
        public void Uses_The_Default_Currency_For_Unmarked_Numeric_Price_Cells()
        {
            const string html = """
                <table><tr><th>Domain</th><th>New Registration</th><th>Renewal</th></tr>
                <tr><td>.com</td><td><del>1.160,29 22,99</del>150,90 2,99</td><td>1.160,29 22,99</td></tr></table>
                """;

            var price = Assert.Single(_parser.Parse(html, "natro"));

            Assert.Equal(2.99m, price.Registration);
            Assert.Equal(22.99m, price.RegularRegistration);
            Assert.Equal(22.99m, price.Renewal);
        }

        [Fact]
        public void Rejects_Pages_Without_A_Recognized_Price_Table()
        {
            Assert.Throws<FormatException>(() => _parser.Parse("<html><body>Challenge</body></html>", "natro"));
        }
    }
}
