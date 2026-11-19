using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Infrastructure.Catalog;
using DomainQueryEngine.PostgreSQL;

namespace DomainQueryEngine.SearchUI.Extensions
{
    public enum DataSourceKind
    {
        /// <summary>Sample data embedded in DomainQueryEngine.Infrastructure/MockData.</summary>
        Mock,

        /// <summary>The application's database.</summary>
        Database
    }

    /// <summary>
    /// "DataSource" section of appsettings: where each area of the UI gets its data. Switching an area to
    /// real data only changes its registration here; pages keep using the same interfaces.
    /// </summary>
    public sealed class DataSourceOptions
    {
        public const string SectionName = "DataSource";

        /// <summary>Extension catalog and prices (<see cref="ITldCatalog"/>). Only <see cref="DataSourceKind.Mock"/> exists so far.</summary>
        public DataSourceKind Tlds { get; set; } = DataSourceKind.Mock;

        /// <summary>Provider profiles (<see cref="IProviderCatalog"/>). Only <see cref="DataSourceKind.Mock"/> exists so far.</summary>
        public DataSourceKind Providers { get; set; } = DataSourceKind.Mock;

        /// <summary>Most searched domains (<see cref="ISearchStatistics"/>), from the lookup history by default.</summary>
        public DataSourceKind Statistics { get; set; } = DataSourceKind.Database;
    }

    public static class DataSourceRegistration
    {
        public static IServiceCollection AddDataSources(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(DataSourceOptions.SectionName).Get<DataSourceOptions>() ?? new DataSourceOptions();

            switch (options.Tlds)
            {
                case DataSourceKind.Mock:
                    services.AddSingleton<ITldCatalog, MockTldCatalog>();
                    break;
                default:
                    throw NotAvailable(nameof(options.Tlds), options.Tlds);
            }

            switch (options.Providers)
            {
                case DataSourceKind.Mock:
                    services.AddSingleton<IProviderCatalog, MockProviderCatalog>();
                    break;
                default:
                    throw NotAvailable(nameof(options.Providers), options.Providers);
            }

            switch (options.Statistics)
            {
                case DataSourceKind.Mock:
                    services.AddSingleton<ISearchStatistics, MockSearchStatistics>();
                    break;
                case DataSourceKind.Database:
                    services.AddScoped<ISearchStatistics, DbSearchStatistics>();
                    break;
            }

            services.AddScoped<TldPriceService>();
            return services;
        }

        private static InvalidOperationException NotAvailable(string area, DataSourceKind kind)
            => new($"{DataSourceOptions.SectionName}:{area} = {kind} is not available yet. Use {DataSourceKind.Mock}.");
    }
}
