using DomainQueryEngine.AAK;
using DomainQueryEngine.Core;
using DomainQueryEngine.GriNet;
using DomainQueryEngine.Ihs;
using DomainQueryEngine.IsimKayit;
using DomainQueryEngine.IsimTescil;
using DomainQueryEngine.Natro;
using DomainQueryEngine.YildizBilisim;
using DomainQueryEngine.YoncuBilisim;
using Microsoft.Extensions.DependencyInjection;

namespace DomainQueryEngine.Infrastructure
{
    //bkz: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
    public static class InfrastructureServiceRegistration
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IDomainLookupStrategy, YildizBilisimLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, YoncuBilisimLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, GriNetLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, AlanAdiKayitLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, NatroLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, IhsLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, IsimKayitLookupStrategy>();
            //services.AddScoped<IDomainLookupStrategy, IsimTescilLookupStrategy>();

            services.AddScoped<YildizBilisimStrategyParser>();
            services.AddScoped<YoncuBilisimStrategyParser>();
            services.AddScoped<GriNetStrategyParser>();
            services.AddScoped<AlanAdiKayitStrategyParser>();
            services.AddScoped<NatroStrategyParser>();
            services.AddScoped<IhsStrategyParser>();
            services.AddScoped<IsimKayitStrategyParser>();
            services.AddScoped<IsimTescilStrategyParser>();

            ServiceProvider = services.BuildServiceProvider();
            return services;
        }
    }
}
