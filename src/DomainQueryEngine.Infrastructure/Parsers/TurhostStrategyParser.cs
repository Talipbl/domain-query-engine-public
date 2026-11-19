using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.Infrastructure.Models;
using DomainQueryEngine.Infrastructure.Parsers;
using Newtonsoft.Json;

namespace DomainQueryEngine.Turhost
{
    public class TurhostStrategyParser : StrategyParserBase
    {
        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var json = StripJsonp(rawResponse);
            var response = JsonConvert.DeserializeObject<Dictionary<string, TurhostDomainModel>>(json);
            if (response == null || response.Count == 0)
                throw new ArgumentException("Invalid response format", nameof(rawResponse));

            return response.Select(entry => ToDomainResult(entry.Key, entry.Value, rawResponse)).ToList();
        }

        /// <summary>
        /// Turhost answers in JSONP even without a callback parameter: <c>/**/callback({...});</c>.
        /// </summary>
        public static string StripJsonp(string rawResponse)
        {
            var start = rawResponse.IndexOf('(');
            var end = rawResponse.LastIndexOf(')');
            return start >= 0 && end > start ? rawResponse.Substring(start + 1, end - start - 1) : rawResponse;
        }

        private DomainResult ToDomainResult(string key, TurhostDomainModel item, string rawResponse)
        {
            var domainName = item.DomainName ?? key;
            var status = item.DomainStatus ?? string.Empty;
            var isAvailable = status == "available";

            var result = new DomainResult
            {
                DomainName = domainName,
                Tld = DomainHelper.GetTldFromDomainName(domainName),
                IsAvailable = isAvailable,
                IsTransferable = status == "not_available",
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (isAvailable && item.Prices != null)
            {
                var regular = item.Prices.GetValueOrDefault("TRY");
                var campaign = item.Prices.GetValueOrDefault("kampanya_try");
                var periods = (regular?.Keys ?? Enumerable.Empty<string>())
                    .Union(campaign?.Keys ?? Enumerable.Empty<string>());

                foreach (var periodKey in periods)
                {
                    if (!int.TryParse(periodKey, out var period))
                        continue;

                    var registration = PreferCampaignPrice(
                        CreatePrice(campaign?.GetValueOrDefault(periodKey), "TRY"),
                        CreatePrice(regular?.GetValueOrDefault(periodKey), "TRY"));

                    if (registration != null)
                        result.Prices.Add(new DomainPrice { Period = period, RegistrationPrice = registration });
                }

                result.Prices.Sort((a, b) => a.Period.CompareTo(b.Period));
            }

            return result;
        }
    }
}
