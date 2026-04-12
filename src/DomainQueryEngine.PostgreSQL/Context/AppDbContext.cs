using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainQueryEngine.PostgreSQL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Domain> Domains => Set<Domain>();
        public DbSet<Provider> Providers => Set<Provider>();
        public DbSet<DomainLookupResult> DomainLookupResults => Set<DomainLookupResult>();
        public DbSet<DomainVisit> DomainVisits => Set<DomainVisit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DomainConfiguration());
            modelBuilder.ApplyConfiguration(new DomainLookupResultConfiguration());
            modelBuilder.ApplyConfiguration(new DomainVisitConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
