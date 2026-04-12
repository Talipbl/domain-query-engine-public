using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Parsers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.IsimTescil
{
    public class IsimTescilStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var results = new List<DomainResult>();
            var doc = new HtmlDocument();
            doc.LoadHtml(rawResponse);

            // her domain için <tr> satırı
            var rows = doc.DocumentNode.SelectNodes("//tr");
            if (rows == null) return results;

            foreach (var tr in rows)
            {
                // domain bilgisi
                var domainNode = tr.SelectSingleNode(".//*[@data-domainname]");
                if (domainNode == null)
                    domainNode = tr.SelectSingleNode(".//b[contains(text(),'.') or contains(text(),'com') or contains(text(),'net') or contains(text(),'org')]");

                if (domainNode == null)
                    continue;

                var domainName = domainNode.GetAttributeValue("data-domainname", null) ??
                                 domainNode.InnerText.Trim();

                var tld = DomainHelper.GetTldFromDomainName(domainName);

                // periyot bilgisi
                var period = domainNode.GetAttributeValue("data-section", null);
                if (string.IsNullOrWhiteSpace(period))
                {
                    var aNode = tr.SelectSingleNode(".//a[@data-section]");
                    period = aNode?.GetAttributeValue("data-section", null);
                }

                // fiyat bilgileri
                var priceNodes = tr.SelectNodes(".//span[contains(text(),'₺') or contains(text(),'$') or contains(text(),'€')]");
                var prices = new List<DomainPrice>();
                if (priceNodes != null)
                {
                    foreach (var p in priceNodes)
                    {
                        var cls = p.GetAttributeValue("class", "").ToLowerInvariant();
                        if (cls.Contains("text-decoration-line-through"))
                            continue; // eski fiyatı geç

                        var text = p.InnerText.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                            prices.Add(new DomainPrice() { Period = Convert.ToInt32(period), RegistrationPrice = TryParsePrice(text)});
                    }
                }

                // durum bilgisi
                var rowText = tr.InnerText.ToLowerInvariant();
                bool isAvailable = rowText.Contains("kayıt") || rowText.Contains("müsait");
                bool isPremium = rowText.Contains("premium");
                bool isTransferable = rowText.Contains("transfer");
                bool isValid = !rowText.Contains("geçersiz");

                var result = new DomainResult
                {
                    DomainName = domainName,
                    Tld = tld,
                    IsAvailable = isAvailable,
                    IsPremium = isPremium,
                    IsTransferable = isTransferable,
                    IsValidDomain = isValid,
                    RawResponse = tr.OuterHtml,
                    Prices = prices
                };

                results.Add(result);
            }

            return results;
        }
    }
}
