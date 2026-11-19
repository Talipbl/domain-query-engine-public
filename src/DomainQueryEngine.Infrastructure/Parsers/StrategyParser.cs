using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace DomainQueryEngine.Infrastructure.Parsers
{
    public abstract class StrategyParserBase : IStrategyParser
    {
        public abstract List<DomainResult> ParseDomainResult(string rawResponse);

        public string GetCsrfTokenFromHtml(string htmlResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlResponse);
            // SelectNodes returns null (not an empty list) when the page has no <script> tags.
            var htmlNodes = doc.DocumentNode.SelectNodes("//script");
            if (htmlNodes == null)
                return string.Empty;

            foreach (var script in htmlNodes)
            {
                var scriptText = script.InnerText;
                if (string.IsNullOrWhiteSpace(scriptText)) continue;

                var m = Regex.Match(scriptText, @"\bcsrfToken\s*=\s*(['""])(?<token>.+?)\1", RegexOptions.Singleline);
                if (m.Success)
                {
                    return m.Groups["token"].Value;
                }
            }
            return string.Empty;
        }

        protected BasePrice? TryParsePrice(string price)
        {
            var (amount, currency) = PriceHelper.Parse(price);
            return amount > 0 ? new BasePrice { Amount = amount, Currency = currency } : null;
        }

        /// <summary>
        /// Builds a price from an amount and a currency code or symbol ("$", "TL", "TRY", "&amp;#x20BA").
        /// Returns null for missing or non-positive amounts (providers use 0 or -1 for "no price").
        /// </summary>
        protected static BasePrice? CreatePrice(decimal? amount, string? currency)
        {
            if (amount is not > 0)
                return null;

            var code = PriceHelper.Parse($"1 {DecodeEntities(currency ?? string.Empty)}").Currency;
            // JSON amounts keep their scale ("8.9900"); prices are shown with two decimals.
            return new BasePrice { Amount = decimal.Round(amount.Value, 2), Currency = code };
        }

        /// <summary>
        /// Decodes HTML entities, including numeric ones sent without the closing ";" ("&amp;#x20BA").
        /// </summary>
        private static string DecodeEntities(string value)
        {
            value = Regex.Replace(value, @"&#x([0-9a-fA-F]+);?", m => char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].Value, 16)));
            value = Regex.Replace(value, @"&#([0-9]+);?", m => char.ConvertFromUtf32(int.Parse(m.Groups[1].Value)));
            return System.Net.WebUtility.HtmlDecode(value);
        }

        /// <summary>
        /// Picks the price to compare: the campaign price when the provider offers one, otherwise the
        /// regular price. In practice that is the lowest positive amount among the candidates.
        /// </summary>
        protected static BasePrice? PreferCampaignPrice(params BasePrice?[] candidates)
            => candidates.Where(p => p is { Amount: > 0 }).OrderBy(p => p!.Amount).FirstOrDefault();

        /// <summary>
        /// Splits a campaign price and a regular price into the price to compare and, when they differ, the
        /// regular price to show next to it. Either may be missing.
        /// </summary>
        protected static (BasePrice? Price, BasePrice? Regular) CampaignAndRegular(BasePrice? campaign, BasePrice? regular)
        {
            var price = PreferCampaignPrice(campaign, regular);
            var higher = new[] { campaign, regular }
                .Where(p => p is { Amount: > 0 } && price != null && p.Amount > price.Amount
                            && string.Equals(p.Currency, price.Currency, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p!.Amount)
                .FirstOrDefault();
            return (price, higher);
        }
    }
}
