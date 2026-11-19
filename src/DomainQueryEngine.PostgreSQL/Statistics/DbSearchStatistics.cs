using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    /// <summary>
    /// Counts searches from the lookup history (one Domains row per search).
    /// </summary>
    public sealed class DbSearchStatistics : ISearchStatistics
    {
        private readonly AppDbContext _context;

        public DbSearchStatistics(AppDbContext context)
        {
            _context = context;
        }

        public bool IsSample => false;

        public async Task<IReadOnlyList<PopularQuery>> GetMostSearchedAsync(TimeSpan window, int take, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow - window;

            var rows = await _context.Domains
                .AsNoTracking()
                .Where(d => d.CreatedAt >= since)
                .GroupBy(d => d.DomainName)
                .Select(g => new { DomainName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.DomainName)
                .Take(take)
                .ToListAsync(ct);

            return rows.Select(r => new PopularQuery(r.DomainName, r.Count)).ToList();
        }

        public async Task<IReadOnlyList<PopularTld>> GetMostSearchedTldsAsync(TimeSpan window, int take, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow - window;

            var rows = await _context.Domains
                .AsNoTracking()
                .Where(d => d.CreatedAt >= since && d.Tld != "")
                .GroupBy(d => d.Tld)
                .Select(g => new { Tld = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Tld)
                .Take(take)
                .ToListAsync(ct);

            return rows.Select(r => new PopularTld(r.Tld.TrimStart('.'), r.Count)).ToList();
        }
    }
}
