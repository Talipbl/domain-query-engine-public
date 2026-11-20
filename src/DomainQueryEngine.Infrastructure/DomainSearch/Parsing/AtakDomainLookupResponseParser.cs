using DomainQueryEngine.Core;
using DomainQueryEngine.Infrastructure.Models;
using DomainQueryEngine.Infrastructure.DomainSearch.Parsing;
using Newtonsoft.Json;

namespace DomainQueryEngine.Infrastructure.DomainSearch.Providers.AtakDomain
{
    public class AtakDomainLookupResponseParser : DomainLookupResponseParserBase
    {
        private const int Available = 1;
        private const int Registered = 2;

        public override List<DomainResult> ParseDomainResult(string rawResponse)
        {
            var response = JsonConvert.DeserializeObject<AtakDomainResponseModel>(rawResponse);
            var inquiry = response?.Data?.Inquiry;
            if (response == null || !response.Success || inquiry?.DomainName == null)
                throw new ArgumentException($"Invalid response format: {response?.Message}", nameof(rawResponse));

            var isAvailable = inquiry.InquiryResultId == Available;
            var isRegistered = inquiry.InquiryResultId == Registered;
            var period = inquiry.Period > 0 ? inquiry.Period : 1;

            var result = new DomainResult
            {
                DomainName = inquiry.DomainName,
                Tld = inquiry.Tld?.TrimStart('.') ?? string.Empty,
                IsAvailable = isAvailable,
                IsTransferable = isRegistered,
                IsValidDomain = isAvailable || isRegistered,
                IsPremium = inquiry.IsPremium,
                RawResponse = rawResponse,
                CheckedAt = DateTime.UtcNow
            };

            if (isAvailable)
            {
                var (registration, regular) = CampaignAndRegular(
                    CreatePrice(inquiry.Price, inquiry.Currency),
                    CreatePrice(inquiry.Selling, inquiry.Currency));

                if (registration != null)
                    result.Prices.Add(new DomainPrice { Period = period, RegistrationPrice = registration, RegularRegistrationPrice = regular });
            }
            else if (isRegistered)
            {
                var transfer = CreatePrice(inquiry.Price, inquiry.Currency);
                if (transfer != null)
                    result.Prices.Add(new DomainPrice { Period = period, TransferPrice = transfer });
            }

            return new List<DomainResult> { result };
        }
    }
}
