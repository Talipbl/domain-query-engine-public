using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.DomainSearch.Parsing;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.HostingComTr
{
    /// <summary>
    /// Parses /web/domain/querynew/lookup responses: <c>{"status":"successs","content":"&lt;html&gt;"}</c>.
    /// Each result is a <c>div.find-item</c> whose button carries data-domain / data-status, with one
    /// &lt;option&gt; per period holding the TRY price in data-price-try ("1 Yıl 449.50₺").
    /// </summary>
    public class HostingComTrLookupResponseParser : DomainLookupResponseParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            JObject response;
            try
            {
                response = JObject.Parse(rawResponse);
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse), ex);
            }

            var content = response.Value<string>("content");
            if (response.Value<string>("status") == "error" || string.IsNullOrEmpty(content))
                throw new ArgumentException($"Lookup failed: {response.Value<string>("message") ?? response.Value<string>("status")}", nameof(rawResponse));

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var items = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' find-item ')][.//*[@data-domain and @data-status]]");
            return (items ?? Enumerable.Empty<HtmlNode>())
                .Select(item => ToDomainResult(item, rawResponse))
                .GroupBy(r => r.DomainName)
                .Select(g => g.First())
                .ToList();
        }

        private DomainResult ToDomainResult(HtmlNode item, string rawResponse)
        {
            var button = item.SelectSingleNode(".//*[@data-domain and @data-status]")!;
            var domainName = button.GetAttributeValue("data-domain", string.Empty);
            var status = button.GetAttributeValue("data-status", string.Empty);
            var isAvailable = status == "available";

            var result = new DomainResult
            {
                DomainName = domainName,
                Tld = DomainHelper.GetTldFromDomainName(domainName),
                IsAvailable = isAvailable,
                IsTransferable = status == "unavailable",
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (isAvailable)
            {
                foreach (var option in item.SelectNodes(".//select[@name='domainperiod[]']/option") ?? Enumerable.Empty<HtmlNode>())
                {
                    if (!int.TryParse(option.GetAttributeValue("value", string.Empty), out var period))
                        continue;

                    // "1 Yıl 449.50₺": drop the period label before reading the amount.
                    var label = HtmlEntity.DeEntitize(option.GetAttributeValue("data-price-try", string.Empty));
                    var yearIndex = label.IndexOf("Yıl", StringComparison.OrdinalIgnoreCase);
                    var price = TryParsePrice(yearIndex >= 0 ? label[(yearIndex + 3)..] : label);

                    if (price != null)
                        result.Prices.Add(new DomainPrice { Period = period, RegistrationPrice = price });
                }
            }

            return result;
        }
    }
}
