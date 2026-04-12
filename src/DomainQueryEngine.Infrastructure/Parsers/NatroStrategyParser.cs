using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.Models;
using DomainQueryEngine.Infrastructure.Parsers;
using Newtonsoft.Json;

namespace DomainQueryEngine.Natro
{
    public class NatroStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var response = JsonConvert.DeserializeObject<NatroLookupResponseModel>(rawResponse)?.ServiceResponse.Api.Response;
            if (response == null)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse));
            }

            var domain = response.Resdata.Domain;

            var result = new List<DomainResult>();

            var domainResult = new DomainResult
            {
                DomainName = domain.Text,
                Tld = domain.Ext,
                IsAvailable = domain.Avail == "1",
                IsValidDomain = response.Msg == "success",
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (domainResult.IsAvailable)
            {
                var price = new DomainPrice()
                {
                    Period = domain.Minyear,
                    RegistrationPrice = TryParsePrice(domain.Price) ?? new BasePrice()
                };

                price.RegistrationPrice.Currency = "USD";
                domainResult.Prices.Add(price);
            }

            result.Add(domainResult);

            return result;
        }
    }
}
