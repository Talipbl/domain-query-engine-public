using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public sealed class TldPriceRepository : ITldPriceCatalog, ITldPriceRepository
    {
        private readonly AppDbContext _db;

        public TldPriceRepository(AppDbContext db) => _db = db;

        public async Task<DateTime> GetAsOfAsync(CancellationToken ct = default)
            => await _db.TldPriceSync.Where(s => s.LastSuccessAt != null)
                .Select(s => s.LastSuccessAt!.Value)
                .OrderBy(t => t)
                .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<TldEntry>> GetAllAsync(CancellationToken ct = default)
        {
            var records = await _db.TldProviderPrices.AsNoTracking().ToListAsync(ct);
            return ToEntries(records);
        }

        public async Task<TldEntry?> GetAsync(string tld, CancellationToken ct = default)
        {
            var key = Normalize(tld);
            var records = await _db.TldProviderPrices.AsNoTracking()
                .Where(p => p.Tld == key || p.Tld.Replace(".", "-") == key)
                .ToListAsync(ct);
            return ToEntries(records).FirstOrDefault();
        }

        public async Task ReplaceProviderPricesAsync(TldPriceSnapshot snapshot, CancellationToken ct = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            await _db.TldProviderPrices.Where(p => p.ProviderSlug == snapshot.ProviderSlug).ExecuteDeleteAsync(ct);

            _db.TldProviderPrices.AddRange(snapshot.Prices.Select(price => new TldProviderPriceRecord
            {
                ProviderSlug = snapshot.ProviderSlug,
                Tld = Normalize(price.Tld),
                Currency = price.Currency.ToUpperInvariant(),
                Registration = price.Registration,
                RegularRegistration = price.RegularRegistration,
                RegistrationTermYears = price.RegistrationTermYears,
                Renewal = price.Renewal,
                RenewalTermYears = price.RenewalTermYears,
                SourceUrl = snapshot.SourceUrl,
                ObservedAt = snapshot.ObservedAt
            }));

            var status = await _db.TldPriceSync.FindAsync([snapshot.ProviderSlug], ct);
            if (status == null)
            {
                status = new TldPriceSyncRecord { ProviderSlug = snapshot.ProviderSlug };
                _db.TldPriceSync.Add(status);
            }
            status.SourceUrl = snapshot.SourceUrl;
            status.LastAttemptAt = snapshot.ObservedAt;
            status.LastSuccessAt = snapshot.ObservedAt;
            status.PriceCount = snapshot.Prices.Count;
            status.LastError = null;

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        public async Task RecordProviderPriceFailureAsync(string providerSlug, string sourceUrl, DateTime attemptedAt, string error, CancellationToken ct = default)
        {
            var status = await _db.TldPriceSync.FindAsync([providerSlug], ct);
            if (status == null)
            {
                status = new TldPriceSyncRecord { ProviderSlug = providerSlug };
                _db.TldPriceSync.Add(status);
            }
            status.SourceUrl = sourceUrl;
            status.LastAttemptAt = attemptedAt;
            status.LastError = error.Length > 1000 ? error[..1000] : error;
            await _db.SaveChangesAsync(ct);
        }

        private static List<TldEntry> ToEntries(IEnumerable<TldProviderPriceRecord> records)
            => records.GroupBy(p => p.Tld, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(group => new TldEntry
                {
                    Tld = group.Key,
                    Prices = group.Select(price => new TldProviderPrice
                    {
                        Tld = group.Key,
                        Provider = price.ProviderSlug,
                        Currency = price.Currency,
                        Registration = price.Registration,
                        RegularRegistration = price.RegularRegistration,
                        RegistrationTermYears = price.RegistrationTermYears,
                        Renewal = price.Renewal,
                        RenewalTermYears = price.RenewalTermYears
                    }).ToList()
                }).ToList();

        private static string Normalize(string tld) => tld.Trim().TrimStart('.').ToLowerInvariant();
    }
}
