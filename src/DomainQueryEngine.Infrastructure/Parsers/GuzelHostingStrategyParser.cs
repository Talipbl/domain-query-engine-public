using System.Text.RegularExpressions;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Parsers;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace DomainQueryEngine.GuzelHosting
{
    /// <summary>
    /// Parses the WHMCS "checkAvailabilityOne" response: <c>{"status":"Available|Taken|...","htmlInput":"&lt;td&gt;..."}</c>.
    /// Prices are listed per period as "1 Yıl @ ₺ 504.29 TL" inside a dropdown.
    /// </summary>
    public class GuzelHostingStrategyParser : StrategyParserBase
    {
        private static readonly Regex PeriodPattern = new(@"addToCart\([^)]*'register',\s*'?(\d+)'?\)", RegexOptions.Compiled);

        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            JObject response;
            try
            {
                response = JObject.Parse(rawResponse);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse), ex);
            }

            var status = response.Value<string>("status");
            var html = response.Value<string>("htmlInput");
            if (string.IsNullOrEmpty(status) || html == null)
                throw new ArgumentException("Invalid response format", nameof(rawResponse));

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var domainName = doc.DocumentNode.SelectSingleNode("//strong")?.InnerText.Trim() ?? string.Empty;
            var isAvailable = status.Equals("Available", StringComparison.OrdinalIgnoreCase);
            var isTaken = status.Equals("Taken", StringComparison.OrdinalIgnoreCase);

            var result = new DomainResult
            {
                DomainName = domainName,
                Tld = string.IsNullOrEmpty(domainName) ? string.Empty : DomainHelper.GetTldFromDomainName(domainName),
                IsAvailable = isAvailable,
                IsTransferable = isTaken,
                IsValidDomain = isAvailable || isTaken,
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (isAvailable)
            {
                var options = doc.DocumentNode.SelectNodes("//a[contains(@onclick,'register')]");
                foreach (var option in options ?? Enumerable.Empty<HtmlNode>())
                {
                    var periodMatch = PeriodPattern.Match(option.GetAttributeValue("onclick", string.Empty));
                    var text = HtmlEntity.DeEntitize(option.InnerText);
                    var priceText = text.Contains('@') ? text[(text.IndexOf('@') + 1)..] : text;
                    var price = TryParsePrice(priceText);

                    if (periodMatch.Success && price != null)
                        result.Prices.Add(new DomainPrice { Period = int.Parse(periodMatch.Groups[1].Value), RegistrationPrice = price });
                }
            }

            return new List<DomainResult> { result };
        }
    }
}
