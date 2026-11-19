using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.Infrastructure.Catalog
{
    /// <summary>
    /// Reads the sample JSON files embedded under MockData/. They are parsed once and kept in memory.
    /// </summary>
    internal static class MockData
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static T Load<T>(string fileName)
        {
            var assembly = typeof(MockData).Assembly;
            using var stream = assembly.GetManifestResourceStream($"MockData.{fileName}")
                ?? throw new InvalidOperationException($"Embedded sample data 'MockData/{fileName}' was not found.");
            return JsonSerializer.Deserialize<T>(stream, Options)
                ?? throw new InvalidOperationException($"Sample data 'MockData/{fileName}' is empty.");
        }

        /// <summary>Accepts "com.tr", ".com.tr", "COM.TR" or the slug "com-tr".</summary>
        public static string NormalizeTld(string tld)
            => tld.Trim().TrimStart('.').Replace('-', '.').ToLower(CultureInfo.InvariantCulture);
    }

    public sealed class MockTldCatalog : ITldCatalog
    {
        private sealed record File(DateTime AsOf, List<TldEntry> Tlds);

        private readonly Lazy<File> _data = new(() => MockData.Load<File>("tlds.json"));

        public bool IsSample => true;
        public DateTime AsOf => _data.Value.AsOf;

        public Task<IReadOnlyList<TldEntry>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<TldEntry>>(_data.Value.Tlds);

        public Task<TldEntry?> GetAsync(string tld, CancellationToken ct = default)
        {
            var key = MockData.NormalizeTld(tld);
            return Task.FromResult(_data.Value.Tlds.FirstOrDefault(t => t.Tld == key));
        }
    }

    public sealed class MockProviderCatalog : IProviderCatalog
    {
        private sealed record File(List<ProviderEntry> Providers);

        private readonly Lazy<File> _data = new(() => MockData.Load<File>("providers.json"));

        public bool IsSample => true;

        public Task<IReadOnlyList<ProviderEntry>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ProviderEntry>>(_data.Value.Providers);

        public Task<ProviderEntry?> GetAsync(string slug, CancellationToken ct = default)
            => Task.FromResult(_data.Value.Providers.FirstOrDefault(p => string.Equals(p.Slug, slug?.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    public sealed class MockSearchStatistics : ISearchStatistics
    {
        private sealed record File(List<PopularQuery> Queries);

        private readonly Lazy<File> _data = new(() => MockData.Load<File>("popular-queries.json"));

        public bool IsSample => true;

        public Task<IReadOnlyList<PopularQuery>> GetMostSearchedAsync(TimeSpan window, int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PopularQuery>>(_data.Value.Queries.OrderByDescending(q => q.Count).Take(take).ToList());

        /// <summary>Adds up the sample queries by extension (everything after the first dot).</summary>
        public Task<IReadOnlyList<PopularTld>> GetMostSearchedTldsAsync(TimeSpan window, int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PopularTld>>(_data.Value.Queries
                .Where(q => q.DomainName.Contains('.'))
                .GroupBy(q => q.DomainName[(q.DomainName.IndexOf('.') + 1)..])
                .Select(g => new PopularTld(g.Key, g.Sum(q => q.Count)))
                .OrderByDescending(t => t.Count)
                .ThenBy(t => t.Tld, StringComparer.Ordinal)
                .Take(take)
                .ToList());
    }
}
