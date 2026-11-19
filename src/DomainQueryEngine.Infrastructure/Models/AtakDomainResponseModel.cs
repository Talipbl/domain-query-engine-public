namespace DomainQueryEngine.Infrastructure.Models
{
    /// <summary>
    /// Response of POST https://www.atakdomain.com/api/domain/getavailability.
    /// </summary>
    public class AtakDomainResponseModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DataModel? Data { get; set; }

        public class DataModel
        {
            public InquiryModel? Inquiry { get; set; }
        }

        public class InquiryModel
        {
            public string? DomainName { get; set; }
            public string? Tld { get; set; }

            /// <summary>1 = available, 2 = registered (transferable). Other values mean the domain cannot be registered.</summary>
            public int InquiryResultId { get; set; }

            public string? Availability { get; set; }
            public int Period { get; set; }

            /// <summary>Price for <see cref="Period"/>: registration when available, transfer when registered.</summary>
            public decimal? Price { get; set; }

            /// <summary>Regular (non-campaign) price; 0 when not offered.</summary>
            public decimal? Selling { get; set; }

            /// <summary>Currency symbol, e.g. "$".</summary>
            public string? Currency { get; set; }

            public bool IsPremium { get; set; }
            public bool DocumentRequired { get; set; }
        }
    }
}
