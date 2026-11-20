using DomainQueryEngine.Infrastructure.DomainSearch.Providers.AAK;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.AtakDomain;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.GriNet;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.HostingComTr;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Ihs;
using DomainQueryEngine.Infrastructure.TldPricing;
using DomainQueryEngine.Infrastructure.ExchangeRates;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.IsimTescil;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Natro;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turhost;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.Turkticaret;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YildizBilisim;
using DomainQueryEngine.Infrastructure.DomainSearch.Providers.YoncuBilisim;
using Microsoft.Extensions.DependencyInjection;

namespace DomainQueryEngine.Infrastructure
{
    //bkz: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddDomainSearchInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IDomainAvailabilityProvider, YildizBilisimDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, YoncuBilisimDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, GriNetDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, AlanAdiKayitDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, NatroDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, IhsDomainAvailabilityProvider>();

            // Disabled: the site moved off WHMCS; index.php?rp=/domain/check returns 403 and there is no
            // server-side availability API any more (the site checks rdap.org in the browser, prices at /api/tlds).
            //services.AddScoped<IDomainAvailabilityProvider, IsimKayitDomainAvailabilityProvider>();
            //services.AddScoped<IDomainAvailabilityProvider, IsimTescilDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, AtakDomainDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, TurhostDomainAvailabilityProvider>();

            // Disabled: Cloudflare answers server-side requests with a bot challenge (cf-mitigated: challenge).
            // We do not work around bot protection; enable this again only if the provider allows it.
            //services.AddScoped<IDomainAvailabilityProvider, GuzelHostingDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, TurkticaretDomainAvailabilityProvider>();
            services.AddScoped<IDomainAvailabilityProvider, HostingComTrDomainAvailabilityProvider>();

            services.AddScoped<YildizBilisimLookupResponseParser>();
            services.AddScoped<YoncuBilisimLookupResponseParser>();
            services.AddScoped<GriNetLookupResponseParser>();
            services.AddScoped<AlanAdiKayitLookupResponseParser>();
            services.AddScoped<NatroLookupResponseParser>();
            services.AddScoped<IhsLookupResponseParser>();
            //services.AddScoped<IsimKayitLookupResponseParser>();
            services.AddScoped<IsimTescilLookupResponseParser>();
            services.AddScoped<AtakDomainLookupResponseParser>();
            services.AddScoped<TurhostLookupResponseParser>();
            //services.AddScoped<GuzelHostingLookupResponseParser>();
            services.AddScoped<TurkticaretLookupResponseParser>();
            services.AddScoped<HostingComTrLookupResponseParser>();

            return services;
        }

        public static IServiceCollection AddTldPricingInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IExchangeRateProvider, TcmbExchangeRateProvider>();
            services.AddSingleton<TldPriceTableParser>();
            services.AddSingleton(new ProviderEntry { Slug = "alan-adi-kayit", Name = "Alan Adı Kayıt", Website = "https://account.aak.com.tr" });
            services.AddSingleton(new ProviderEntry { Slug = "atak-domain", Name = "Atak Domain", Website = "https://atakdomain.com" });
            services.AddSingleton(new ProviderEntry { Slug = "grinet", Name = "GriNet", Website = "https://www.gri.net" });
            services.AddSingleton(new ProviderEntry { Slug = "hosting-com-tr", Name = "Hosting.com.tr", Website = "https://www.hosting.com.tr" });
            services.AddSingleton(new ProviderEntry { Slug = "ihs", Name = "IHS", Website = "https://www.ihs.com.tr" });
            services.AddSingleton(new ProviderEntry { Slug = "isim-kayit", Name = "İsim Kayıt", Website = "https://www.isimkayit.com" });
            services.AddSingleton(new ProviderEntry { Slug = "natro", Name = "Natro", Website = "https://www.natro.com" });
            services.AddSingleton(new ProviderEntry { Slug = "turhost", Name = "Turhost", Website = "https://www.turhost.com" });
            services.AddSingleton(new ProviderEntry { Slug = "turkticaret", Name = "Turkticaret.net", Website = "https://www.turkticaret.net" });
            services.AddSingleton(new ProviderEntry { Slug = "yoncu-bilisim", Name = "Yöncü Bilişim", Website = "https://www.yoncu.com" });
            services.AddScoped<ITldPriceSource, AakTldPriceSource>();
            services.AddScoped<ITldPriceSource, AtakDomainTldPriceSource>();
            services.AddScoped<ITldPriceSource, GriNetTldPriceSource>();
            services.AddScoped<ITldPriceSource, HostingComTrTldPriceSource>();
            services.AddScoped<ITldPriceSource, IhsTldPriceSource>();
            services.AddScoped<ITldPriceSource, NatroTldPriceSource>();
            services.AddScoped<ITldPriceSource, TurkticaretTldPriceSource>();
            services.AddScoped<ITldPriceSource, TurhostTldPriceSource>();
            services.AddScoped<ITldPriceSource, YoncuTldPriceSource>();
            services.AddScoped<ITldPriceSource, IsimKayitTldPriceSource>();

            return services;
        }
    }
}
