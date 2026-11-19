using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Parsers;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace DomainQueryEngine.YoncuBilisim
{
    public class YoncuBilisimStrategyParser : StrategyParserBase
    {
        private const int StatusAvailable = 1;
        private const int StatusForbidden = 9;

        /// <summary>Current time; replaced in tests to check campaign expiry.</summary>
        public Func<DateTimeOffset> Now { get; init; } = () => DateTimeOffset.UtcNow;

        /// <summary>Reads the <c>window.cftoken = "..."</c> value the API expects in the "cftoken" header.</summary>
        public static string GetCfToken(string html)
        {
            var match = Regex.Match(html ?? string.Empty, @"window\.cftoken\s*=\s*([""'])(?<token>[^""']+)\1");
            return match.Success ? match.Groups["token"].Value : string.Empty;
        }

        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            if (JToken.Parse(rawResponse) is not JArray { Count: >= 2 } root || root[0].Type != JTokenType.Boolean)
                throw new ArgumentException("Invalid response format", nameof(rawResponse));

            if (!root[0].Value<bool>() || root[1] is not JObject domains)
                throw new InvalidOperationException($"Yöncü returned an error: {root[1]}");

            var results = new List<DomainResult>();
            foreach (var (domainName, value) in domains)
            {
                if (value is not JObject item)
                    continue;

                var status = item.Value<int?>("status");
                var isAvailable = status == StatusAvailable;
                var isTransferable = item["transfer"]?.Value<int?>("status") == 1;
                var tldInfo = item["tldinfo"] as JObject; // an empty array when the TLD is unknown or forbidden

                var result = new DomainResult
                {
                    DomainName = domainName,
                    Tld = tldInfo?.Value<string>("tld") ?? DomainHelper.GetTldFromDomainName(domainName),
                    IsAvailable = isAvailable,
                    IsTransferable = isTransferable,
                    IsValidDomain = status != StatusForbidden,
                    RawResponse = item.ToString(Newtonsoft.Json.Formatting.None),
                    CheckedAt = DateTime.UtcNow
                };

                if (tldInfo != null && (isAvailable || isTransferable))
                    result.Prices.Add(BuildPrice(tldInfo, isAvailable));

                results.Add(result);
            }

            return results;
        }

        private DomainPrice BuildPrice(JObject tld, bool includeRegistration)
        {
            var currency = tld.Value<string>("currency");
            var regular = CreatePrice(tld.Value<decimal?>("register"), currency);

            // campaign_register stays filled after a campaign ends; only trust it before campaign_expiry.
            var expiry = tld.Value<long?>("campaign_expiry");
            var campaignActive = expiry is > 0 && DateTimeOffset.FromUnixTimeSeconds(expiry.Value) > Now();
            var campaign = campaignActive ? CreatePrice(tld.Value<decimal?>("campaign_register"), currency) : null;
            var (price, regularPrice) = CampaignAndRegular(campaign, regular);

            return new DomainPrice
            {
                Period = Math.Max(1, tld.Value<int?>("minrenew") ?? 1),
                RegistrationPrice = includeRegistration ? price! : null!,
                RegularRegistrationPrice = includeRegistration ? regularPrice : null,
                RenewalPrice = CreatePrice(tld.Value<decimal?>("renew"), currency)!,
                TransferPrice = CreatePrice(tld.Value<decimal?>("transfer"), currency)!
            };
        }
    }
}
