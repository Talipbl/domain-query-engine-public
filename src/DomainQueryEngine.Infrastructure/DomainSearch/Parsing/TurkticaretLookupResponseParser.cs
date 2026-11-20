using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.DomainSearch.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turkticaret
{
    /// <summary>
    /// Parses smartquery_v2.php responses. <c>main_dom</c> holds one entry per queried domain
    /// (an array for "dquery", sometimes an object keyed by domain for "trquery").
    /// </summary>
    public class TurkticaretLookupResponseParser : DomainLookupResponseParserBase
    {
        /// <summary>
        /// Returned instead of JSON when the site wants the visitor to solve its image challenge.
        /// We report the lookup as failed; we never try to solve the challenge.
        /// </summary>
        public const string SecurityChallengeResponse = "E_KEY_ERROR";

        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            if (rawResponse.Trim() == SecurityChallengeResponse)
                throw new InvalidOperationException("Turkticaret asked for a security challenge (E_KEY_ERROR).");

            JObject response;
            try
            {
                response = JObject.Parse(rawResponse);
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException("Invalid response format", nameof(rawResponse), ex);
            }

            var entries = response["main_dom"] switch
            {
                JArray array => array.OfType<JObject>(),
                JObject map => map.Properties().Select(p => p.Value).OfType<JObject>(),
                _ => throw new ArgumentException("Invalid response format", nameof(rawResponse))
            };

            return entries.Select(entry => ToDomainResult(entry, rawResponse)).ToList();
        }

        private static DomainResult ToDomainResult(JObject entry, string rawResponse)
        {
            var domainName = entry.Value<string>("domain_utf8") ?? entry.Value<string>("domain_xn") ?? string.Empty;
            var status = entry.Value<string>("status") ?? string.Empty;
            var isAvailable = status == "available";

            // "regthroughothers": registered elsewhere, "exists": registered .tr domain.
            var isRegistered = status is "regthroughothers" or "exists";

            var result = new DomainResult
            {
                DomainName = domainName,
                Tld = (entry.Value<string>("tld") ?? DomainTld(domainName)).TrimStart('.'),
                IsAvailable = isAvailable,
                IsTransferable = isRegistered,
                IsValidDomain = isAvailable || isRegistered,
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (isAvailable)
            {
                var currency = entry.Value<string>("currency");
                // "price_raw" is the campaign price (first purchase); "campaign_price" is, despite the name, the regular one.
                var (registration, regular) = CampaignAndRegular(
                    CreatePrice(entry.Value<decimal?>("price_raw"), currency),
                    CreatePrice(ParseDecimal(entry.Value<string>("campaign_price")), currency));

                if (registration != null)
                    result.Prices.Add(new DomainPrice { Period = 1, RegistrationPrice = registration, RegularRegistrationPrice = regular });
            }

            return result;
        }

        private static string DomainTld(string domainName)
        {
            var dot = domainName.IndexOf('.');
            return dot >= 0 ? domainName[(dot + 1)..] : string.Empty;
        }

        private static decimal? ParseDecimal(string? value)
            => decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
    }
}
