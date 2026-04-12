namespace DomainQueryEngine.Core.Entities
{
    public class DomainVisit
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DomainId { get; set; }
        public Guid ProviderId { get; set; }
        public string VisitLink { get; set; }
        public DateTime VisitedAt { get; set; } = DateTime.Now;

        public virtual Domain Domain { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
