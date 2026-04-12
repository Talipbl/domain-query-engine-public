using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Infrastructure.Models
{
    public class LookupResponseModel
    {
        public string DomainName { get; set; }
        public string IdnDomainName { get; set; }
        public string Tld { get; set; }
        public string TldNoDots { get; set; }
        public string Sld { get; set; }
        public string IdnSld { get; set; }
        public string Status { get; set; }
        public string LegacyStatus { get; set; }
        public int Score { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsValidDomain { get; set; }
        public string DomainErrorMessage { get; set; }
        public Dictionary<int, PricingModel> Pricing { get; set; }
        public ShortestPeriodModel ShortestPeriod { get; set; }
        public string Group { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool IsPremium { get; set; }
        public List<PremiumCostPricingModel> PremiumCostPricing { get; set; }

        public class PricingModel
        {
            public string Register { get; set; }
            public string Transfer { get; set; }
            public string Renewal { get; set; }
        }
        public class ShortestPeriodModel
        {
            public int Period { get; set; }
            public string Register { get; set; }
            public string Transfer { get; set; }
            public string Renew { get; set; }
        }

        public class PremiumCostPricingModel
        {
            public string Register { get; set; }
            public string Renew { get; set; }
            public string CurrencyCode { get; set; }
        }
    }
}
