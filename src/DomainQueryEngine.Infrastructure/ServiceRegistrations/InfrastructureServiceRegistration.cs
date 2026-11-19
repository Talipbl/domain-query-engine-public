using DomainQueryEngine.AAK;
using DomainQueryEngine.AtakDomain;
using DomainQueryEngine.Core;
using DomainQueryEngine.GriNet;
using DomainQueryEngine.GuzelHosting;
using DomainQueryEngine.HostingComTr;
using DomainQueryEngine.Ihs;
using DomainQueryEngine.IsimKayit;
using DomainQueryEngine.IsimTescil;
using DomainQueryEngine.Natro;
using DomainQueryEngine.Turhost;
using DomainQueryEngine.Turkticaret;
using DomainQueryEngine.YildizBilisim;
using DomainQueryEngine.YoncuBilisim;
using Microsoft.Extensions.DependencyInjection;

namespace DomainQueryEngine.Infrastructure
{
    //bkz: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
    public static class InfrastructureServiceRegistration
    {
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
            services.AddScoped<IDomainLookupStrategy, AtakDomainLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, TurhostLookupStrategy>();
            // Disabled: Cloudflare answers server-side requests with a bot challenge (cf-mitigated: challenge).
            // We do not work around bot protection; enable this again only if the provider allows it.
            //services.AddScoped<IDomainLookupStrategy, GuzelHostingLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, TurkticaretLookupStrategy>();
            services.AddScoped<IDomainLookupStrategy, HostingComTrLookupStrategy>();

            services.AddScoped<YildizBilisimStrategyParser>();
            services.AddScoped<YoncuBilisimStrategyParser>();
            services.AddScoped<GriNetStrategyParser>();
            services.AddScoped<AlanAdiKayitStrategyParser>();
            services.AddScoped<NatroStrategyParser>();
            services.AddScoped<IhsStrategyParser>();
            services.AddScoped<IsimKayitStrategyParser>();
            services.AddScoped<IsimTescilStrategyParser>();
            services.AddScoped<AtakDomainStrategyParser>();
            services.AddScoped<TurhostStrategyParser>();
            //services.AddScoped<GuzelHostingStrategyParser>();

            services.AddSingleton<Core.Abstractions.IExchangeRateProvider, ExchangeRates.TcmbExchangeRateProvider>();
            services.AddScoped<TurkticaretStrategyParser>();
            services.AddScoped<HostingComTrStrategyParser>();
            return services;
        }
    }
}
