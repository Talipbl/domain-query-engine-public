using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace DomainQueryEngine.PostgreSQL
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            builder.ToTable("Providers");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.BaseUrl)
                .HasMaxLength(500);

            builder.Property(p => p.VisitLinkTemplate)
                .HasMaxLength(500);

            builder.Property(p => p.LogoUrl)
                .HasMaxLength(500);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(p => p.Name)
                .IsUnique();

            builder.HasMany(p => p.LookupResults)
                .WithOne(l => l.Provider)
                .HasForeignKey(l => l.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Visits)
                .WithOne(v => v.Provider)
                .HasForeignKey(v => v.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
