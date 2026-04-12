using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Core
{
    public class DomainResult
    {
        public string DomainName { get; set; }
        public string Tld { get; set; }
        public bool IsSuccess { get; set; } = true;
        public bool IsAvailable { get; set; }
        public bool IsPremium { get; set; } = false;
        public bool IsValidDomain { get; set; } = true;
        public bool IsTransferable { get; set; } = false;
        public string RawResponse { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.Now;
        public List<DomainPrice> Prices { get; set; } = new();
    }
}
