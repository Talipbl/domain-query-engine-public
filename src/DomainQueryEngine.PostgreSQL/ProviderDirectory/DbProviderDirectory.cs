using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    /// <summary>Combines configured provider metadata with price collection status from PostgreSQL.</summary>
    public sealed class DbProviderDirectory : IProviderDirectory
    {
        private readonly AppDbContext _db;
        private readonly IReadOnlyList<ProviderEntry> _configuredProviders;

        public DbProviderDirectory(AppDbContext db, IEnumerable<ProviderEntry> providers)
        {
            _db = db;
            _configuredProviders = providers
                .DistinctBy(provider => provider.Slug, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<IReadOnlyList<ProviderEntry>> GetAllAsync(CancellationToken ct = default)
        {
            var statuses = await _db.TldPriceSync.AsNoTracking().ToDictionaryAsync(status => status.ProviderSlug, ct);
            var counts = await _db.TldProviderPrices.AsNoTracking()
                .GroupBy(price => price.ProviderSlug)
                .Select(group => new { ProviderSlug = group.Key, Count = group.Count() })
                .ToDictionaryAsync(group => group.ProviderSlug, group => group.Count, ct);

            return _configuredProviders.Select(provider => new ProviderEntry
            {
                Slug = provider.Slug,
                Name = provider.Name,
                Website = provider.Website,
                LogoUrl = provider.LogoUrl,
                Status = statuses.TryGetValue(provider.Slug, out var sync) && sync.LastError != null
                    ? ProviderStatus.Failing : ProviderStatus.Active,
                TldCount = counts.GetValueOrDefault(provider.Slug)
            }).ToList();
        }

        public async Task<ProviderEntry?> GetAsync(string slug, CancellationToken ct = default)
            => (await GetAllAsync(ct)).FirstOrDefault(provider =>
                string.Equals(provider.Slug, slug?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
