using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.Application.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        public DomainService(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }
        public async Task<Domain> AddDomainSearch(Domain domain)
        {
            return await _domainRepository.AddAsync(domain);
        }
    }
}
