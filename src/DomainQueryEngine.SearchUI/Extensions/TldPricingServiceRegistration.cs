using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.SearchUI.Hosting.TldPricing;

namespace DomainQueryEngine.SearchUI.Extensions
{
    public static class TldPricingServiceRegistration
    {
        public static IServiceCollection AddTldPricingServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<TldPriceCollectionService>();
            services.Configure<TldPriceCollectionOptions>(configuration.GetSection(TldPriceCollectionOptions.SectionName));
            services.AddHostedService<TldPriceCollectionHostedService>();
            services.AddScoped<TldPriceService>();
            return services;
        }
    }
}
