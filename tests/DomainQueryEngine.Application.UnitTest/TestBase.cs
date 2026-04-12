using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Core;
using DomainQueryEngine.GriNet;
using DomainQueryEngine.YildizBilisim;
using DomainQueryEngine.YoncuBilisim;
using Microsoft.Extensions.DependencyInjection;

namespace DomainQueryEngine.Application.Test
{
    public class TestBase
    {
        private ServiceProvider _services;
        protected ILookupService LookupService => _services.GetRequiredService<ILookupService>();

        public TestBase()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDomainLookupStrategy, YildizBilisimLookupStrategy>();
            services.AddSingleton<IDomainLookupStrategy, YoncuBilisimLookupStrategy>();
            services.AddSingleton<IDomainLookupStrategy, GriNetLookupStrategy>();
            services.AddSingleton<YildizBilisimStrategyParser>();
            services.AddSingleton<YoncuBilisimStrategyParser>();
            services.AddSingleton<GriNetStrategyParser>();
            services.AddSingleton<ILookupService, LookupService>();

            _services = services.BuildServiceProvider();
        }
    }
}