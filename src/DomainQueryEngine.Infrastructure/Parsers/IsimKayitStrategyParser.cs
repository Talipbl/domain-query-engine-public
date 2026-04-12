using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.Models;
using DomainQueryEngine.Infrastructure.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DomainQueryEngine.Natro.NatroLookupResponseModel;

namespace DomainQueryEngine.IsimKayit
{
    public class IsimKayitStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var response = JsonConvert.DeserializeObject<BaseResponseModel<LookupResponseModel>>(rawResponse)?.Result;
            if (response == null)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse));
            }

            var results = new List<DomainResult>();

            foreach (var item in response)
            {
                var domain = new DomainResult
                {
                    DomainName = item.DomainName,
                    Tld = item.Tld,
                    IsAvailable = item.IsAvailable,
                    IsValidDomain = item.IsValidDomain,
                    IsPremium = item.IsPremium,
                    RawResponse = JsonConvert.SerializeObject(item),
                    CheckedAt = DateTime.Now
                };

                if (item.Pricing != null)
                {
                    foreach (var kvp in item.Pricing)
                    {
                        int period = kvp.Key;
                        var priceInfo = kvp.Value;

                        var domainPrice = new DomainPrice
                        {
                            Period = period,
                            RegistrationPrice = TryParsePrice(priceInfo.Register),
                            RenewalPrice = TryParsePrice(priceInfo.Renewal),
                            TransferPrice = TryParsePrice(priceInfo.Transfer)
                        };

                        domain.Prices.Add(domainPrice);
                    }
                }

                if (item.ShortestPeriod != null && !domain.Prices.Any(p => p.Period == item.ShortestPeriod.Period))
                {
                    domain.Prices.Add(new DomainPrice
                    {
                        Period = item.ShortestPeriod.Period,
                        RegistrationPrice = TryParsePrice(item.ShortestPeriod.Register),
                        RenewalPrice = TryParsePrice(item.ShortestPeriod.Renew),
                        TransferPrice = TryParsePrice(item.ShortestPeriod.Transfer)
                    });
                }

                results.Add(domain);
            }

            return results;
        }
    }
}
