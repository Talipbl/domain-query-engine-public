using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Core.Entities
{
    public class Domain
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DomainName { get; set; } = null!;
        public string Tld { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UserIp { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }

        public virtual ICollection<DomainLookupResult> LookupResults { get; set; }
        public virtual ICollection<DomainVisit> Visits { get; set; }
    }
}
