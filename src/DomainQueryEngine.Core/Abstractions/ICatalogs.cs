using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.Core.Abstractions
{
    /// <summary>
    /// Data behind the extension price pages. The first implementation reads sample data; a real one
    /// can replace it through DI without touching the pages.
    /// </summary>
    public interface ITldCatalog
    {
        /// <summary>True when the data is sample data; pages then mark it as such.</summary>
        bool IsSample { get; }

        /// <summary>When the data was collected.</summary>
        DateTime AsOf { get; }

        Task<IReadOnlyList<TldEntry>> GetAllAsync(CancellationToken ct = default);

        /// <param name="tld">With or without the leading dot, or as a slug ("com-tr").</param>
        Task<TldEntry?> GetAsync(string tld, CancellationToken ct = default);
    }

    public interface IProviderCatalog
    {
        bool IsSample { get; }

        Task<IReadOnlyList<ProviderEntry>> GetAllAsync(CancellationToken ct = default);

        Task<ProviderEntry?> GetAsync(string slug, CancellationToken ct = default);
    }

    public interface ISearchStatistics
    {
        bool IsSample { get; }

        /// <summary>The most searched domains within <paramref name="window"/>, most searched first.</summary>
        Task<IReadOnlyList<PopularQuery>> GetMostSearchedAsync(TimeSpan window, int take, CancellationToken ct = default);
    }
}
