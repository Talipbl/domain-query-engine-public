namespace DomainQueryEngine.Core
{
    public class DomainPrice
    {
        public int Period { get; set; }
        public BasePrice RegistrationPrice { get; set; }
        public BasePrice RenewalPrice { get; set; }
        public BasePrice TransferPrice { get; set; }
    }
}
