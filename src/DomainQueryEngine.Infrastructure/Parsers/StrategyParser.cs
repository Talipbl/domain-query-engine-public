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
            var htmlNodes = doc.DocumentNode.SelectNodes("//script");
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
    }
}
