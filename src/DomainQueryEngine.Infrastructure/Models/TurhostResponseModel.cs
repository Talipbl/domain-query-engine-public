using Newtonsoft.Json;

namespace DomainQueryEngine.Infrastructure.Models
{
    /// <summary>
    /// One entry of the Turhost domain query response, which is keyed by domain name:
    /// <c>callback({"example.com": { ... }})</c>.
    /// </summary>
    public class TurhostDomainModel
    {
        [JsonProperty("domain_name")]
        public string? DomainName { get; set; }

        /// <summary>"available", "not_available" or "unable_to_register".</summary>
        [JsonProperty("domain_status")]
        public string? DomainStatus { get; set; }

        /// <summary>
        /// Prices per currency and period ("1".."5"). Keys: "TRY"/"USD" (regular),
        /// "kampanya_try"/"kampanya_usd" (campaign), "liste_try"/"liste_usd" (list price).
        /// "-1.00" means the period is not offered.
        /// </summary>
        [JsonProperty("prices")]
        public Dictionary<string, Dictionary<string, decimal>>? Prices { get; set; }
    }
}
