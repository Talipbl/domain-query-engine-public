using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Parsers;
using HtmlAgilityPack;

namespace DomainQueryEngine.YoncuBilisim
{
    public class YoncuBilisimStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string htmlResponse)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlResponse);

            var node = doc.DocumentNode;
            var domainNode = node.SelectSingleNode("//code");
            var domainName = domainNode?.InnerText?.Trim() ?? string.Empty;
            var tld = DomainHelper.GetTldFromDomainName(domainName);

            var availableText = new string[] { "boş (!)", "boş" }; 
            var statusNode = node.SelectSingleNode("//strong") ?? node.SelectSingleNode("//b");
            var isAvailable = availableText.Contains(statusNode?.InnerText?.Trim().ToLower());

            var priceOptions = new List<DomainPrice>();

            var isPremium = statusNode?.InnerText?.Trim().ToLower() == "premium";
            if (isPremium)
            {
                var premiumNode = node.SelectSingleNode("//h5[contains(text(),'Premium')]");
                if (premiumNode != null)
                {
                    priceOptions.Add(new DomainPrice()
                    {
                        Period = 1,
                        RegistrationPrice = TryParsePrice(premiumNode.InnerText) ?? new()
                    });
                }
            }

            var transferNode = node.SelectSingleNode("//h2[contains(text(), 'Transfer Yap')]");
            bool isTransferable = transferNode != null;
            if (transferNode != null)
            {
                var priceNode = node.SelectSingleNode("//h1");
                if (priceNode != null)
                {
                    var priceText = priceNode.InnerText
                        .Replace("&nbsp;", " ")
                        .Trim();

                    priceOptions.Add(new DomainPrice()
                    {
                        Period = 1,
                        TransferPrice = TryParsePrice(priceText) ?? new()
                    });
                }
            }

            var selectNode = node.SelectSingleNode("//select");
            var options = selectNode?.SelectNodes(".//option");

            if (options != null)
            {
                foreach (var opt in options)
                {
                    var text = opt.InnerText
                        .Replace("&nbsp;", " ")
                        .Trim();

                    var parts = text.Split('/', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim()).ToArray();

                    if (parts.Length >= 2)
                    {
                        var yearStr = new string(parts[0].Where(char.IsDigit).ToArray());
                        var priceStr = new string(parts[1].Split("=").Count() > 1 ? parts[1].Split("=")[1].ToArray() : parts[1].Split("=")[0].ToArray());

                        if (int.TryParse(yearStr, out var year))
                        {
                            priceOptions.Add(new DomainPrice
                            {
                                Period = year,
                                RegistrationPrice = TryParsePrice(priceStr) ?? new()
                            });
                        }
                    }
                }
            }

            return new List<DomainResult>()
            {
                new()
                {
                    DomainName = domainName,
                    Tld = tld,
                    CheckedAt = DateTime.Now,
                    IsAvailable = isAvailable,
                    IsPremium = isPremium,
                    IsTransferable = isTransferable,
                    IsValidDomain = true,
                    Prices = priceOptions,
                    RawResponse = htmlResponse
                }
            };
        }
    }
}