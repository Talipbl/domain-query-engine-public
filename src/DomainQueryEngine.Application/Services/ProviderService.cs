using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Entities;

namespace DomainQueryEngine.Application.Lookup
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        public ProviderService(IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        public async Task<Provider> GetOrCreateProviderAsync(ProviderInfo info)
        {
            var existing = await _providerRepository.GetByNameAsync(info.Name);

            if (existing != null)
            {
                var changed = false;

                if (!string.IsNullOrWhiteSpace(info.Url) && existing.BaseUrl != info.Url)
                { existing.BaseUrl = info.Url; changed = true; }

                if (!string.IsNullOrWhiteSpace(info.LogoUrl) && existing.LogoUrl != info.LogoUrl)
                { existing.LogoUrl = info.LogoUrl; changed = true; }

                if (!string.IsNullOrWhiteSpace(info.VisitLink) && existing.VisitLinkTemplate != info.VisitLink)
                { existing.VisitLinkTemplate = info.VisitLink; changed = true; }

                if (changed)
                    _providerRepository.Update(existing);

                return existing;
            }

            var provider = new Provider
            {
                Name = info.Name,
                BaseUrl = info.Url ?? "",
                LogoUrl = info.LogoUrl ?? "",
                VisitLinkTemplate = info.VisitLink ?? "",
                IsActive = true
            };

            await _providerRepository.AddAsync(provider);
            return provider;
        }
    }
}
