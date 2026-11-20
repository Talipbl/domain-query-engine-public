using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainQueryEngine.PostgreSQL
{
    public sealed class TldProviderPriceConfiguration : IEntityTypeConfiguration<TldProviderPriceRecord>
    {
        public void Configure(EntityTypeBuilder<TldProviderPriceRecord> builder)
        {
            builder.ToTable("TldProviderPrices");
            builder.HasKey(p => new { p.ProviderSlug, p.Tld });
            builder.Property(p => p.ProviderSlug).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Tld).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            builder.Property(p => p.Registration).HasPrecision(18, 4);
            builder.Property(p => p.RegularRegistration).HasPrecision(18, 4);
            builder.Property(p => p.Renewal).HasPrecision(18, 4);
            builder.Property(p => p.SourceUrl).HasMaxLength(1000).IsRequired();
            builder.HasIndex(p => p.Tld);
            builder.HasIndex(p => p.ObservedAt);
        }
    }

    public sealed class TldPriceSyncConfiguration : IEntityTypeConfiguration<TldPriceSyncRecord>
    {
        public void Configure(EntityTypeBuilder<TldPriceSyncRecord> builder)
        {
            builder.ToTable("TldPriceSync");
            builder.HasKey(p => p.ProviderSlug);
            builder.Property(p => p.ProviderSlug).HasMaxLength(100).IsRequired();
            builder.Property(p => p.SourceUrl).HasMaxLength(1000).IsRequired();
            builder.Property(p => p.LastError).HasMaxLength(1000);
        }
    }
}
