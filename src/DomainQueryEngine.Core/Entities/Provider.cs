namespace DomainQueryEngine.Core.Entities
{
    public class Provider
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string VisitLinkTemplate { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<DomainLookupResult> LookupResults { get; set; }
        public virtual ICollection<DomainVisit> Visits { get; set; }
    }
}
