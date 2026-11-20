using DomainQueryEngine.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainQueryEngine.PostgreSQL
{
    //bkz: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
    public static class PostgreSQLServiceRegistration
    {

        public static IServiceCollection AddPostgreSQLServices(this IServiceCollection services)
        {
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<IDomainLookupResultRepository, DomainLookupResultRepository>();
            services.AddScoped<IDomainVisitRepository, DomainVisitRepository>();
            services.AddScoped<TldPriceRepository>();
            services.AddScoped<ITldPriceCatalog>(sp => sp.GetRequiredService<TldPriceRepository>());
            services.AddScoped<ITldPriceRepository>(sp => sp.GetRequiredService<TldPriceRepository>());
            services.AddScoped<IProviderDirectory, DbProviderDirectory>();
            services.AddScoped<ISearchStatistics, DbSearchStatistics>();
            return services;
        }
    }
}
