using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.Models;
using DomainQueryEngine.Infrastructure.Parsers;
using Newtonsoft.Json;

namespace DomainQueryEngine.GriNet
{
    public class GriNetStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var response = JsonConvert.DeserializeObject<BaseResponseModel<LookupResponseModel>>(rawResponse)?.Result;
            if (response == null)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse));
            }

            var result = new List<DomainResult>();

            foreach (var item in response)
            {
                var domainResult = new DomainResult
                {
                    DomainName = item.DomainName,
                    Tld = item.Tld,
                    IsAvailable = item.IsAvailable,
                    IsValidDomain = item.IsValidDomain,
                    RawResponse = rawResponse,
                    CheckedAt = DateTime.UtcNow,
                    IsPremium = item.IsPremium
                };

                domainResult.Prices = item.Pricing?.Select(p => new DomainPrice
                {
                    Period = p.Key,
                    RegistrationPrice = TryParsePrice(p.Value.Register) ?? new BasePrice(),
                    TransferPrice = TryParsePrice(p.Value.Transfer) ?? new BasePrice(),
                    RenewalPrice = TryParsePrice(p.Value.Renewal) ?? new BasePrice()
                }).ToList() ?? new List<DomainPrice>();

                result.Add(domainResult);
            }

            return result;
        }
    }
}
