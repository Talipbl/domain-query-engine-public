using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Core
{
    public interface IDomainLookupStrategy
    {
        public ProviderInfo Provider { get; set; }
        public AuthenticationInfo Authentication { get; set; }
        Task<DomainResult> LookupAsync(string domainName, CancellationToken ct = default);
        Task HandshakeAsync();
    }
}
