using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Parsers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Ihs
{
    public class IhsStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawResponse);

            var domainResults = new List<DomainResult>();

            // Musait ve sepette divlerini seçiyoruz
            var domainNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'domain__box__query__results--item') and (contains(@id,'musait') or contains(@id,'altmusait'))]");

            if (domainNodes != null)
            {
                foreach (var node in domainNodes)
                {
                    var domainNameNode = node.SelectSingleNode(".//span[contains(@class,'domain__box__query__results--item-domain-name') or contains(@class,'domain__box__query__results--fast-search-results-domanin-name')]");
                    if (domainNameNode == null) continue;

                    var domainName = domainNameNode.InnerText.Trim();
                    var lastDotIndex = domainName.LastIndexOf('.');
                    var tld = DomainHelper.GetTldFromDomainName(domainName);

                    var priceNodes = node.SelectNodes(".//span[contains(@class,'show-us-curr') or contains(@class,'show-tl-curr')]");
                    var prices = new List<DomainPrice>();

                    if (priceNodes != null)
                    {
                        var tlNode = node.SelectSingleNode(".//span[contains(@class,'show-tl-curr')]");
                        var tlPrice = TryParsePrice(tlNode?.InnerText);

                        var domainPrice = new DomainPrice
                        {
                            Period = 1,
                            RegistrationPrice = new BasePrice { Amount = tlPrice.Amount, Currency = tlPrice.Currency }
                        };

                        prices.Add(domainPrice);
                    }

                    var isAvailable = node.GetClasses().Any(c => c.Contains("available"));

                    domainResults.Add(new DomainResult
                    {
                        DomainName = domainName,
                        Tld = tld,
                        IsAvailable = isAvailable,
                        Prices = prices,
                        RawResponse = node.OuterHtml
                    });
                }
            }

            return domainResults;
        }
    }
}
