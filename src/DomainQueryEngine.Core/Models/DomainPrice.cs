namespace DomainQueryEngine.Core
{
    public class DomainPrice
    {
        public int Period { get; set; }
        /// <summary>The price to compare: the campaign price when the provider offers one.</summary>
        public BasePrice RegistrationPrice { get; set; }

        /// <summary>
        /// The provider's regular registration price when <see cref="RegistrationPrice"/> is a campaign price;
        /// null when there is no campaign.
        /// </summary>
        public BasePrice? RegularRegistrationPrice { get; set; }

        public BasePrice RenewalPrice { get; set; }
        public BasePrice TransferPrice { get; set; }
    }
}
