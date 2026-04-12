namespace DomainQueryEngine.Core.Entities
{
    public class DomainLookupResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DomainId { get; set; }
        public Guid ProviderId { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsAvailable { get; set; }
        public long ResponseTimeMs { get; set; }
        public string RawResponse { get; set; } = null!;

        public decimal? PriceAmount { get; set; }
        public string? PriceCurrency { get; set; }
        public int? PricePeriod { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.Now;

        public virtual Domain Domain { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
