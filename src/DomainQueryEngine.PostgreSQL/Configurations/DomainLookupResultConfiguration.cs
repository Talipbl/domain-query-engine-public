using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainLookupResultConfiguration : IEntityTypeConfiguration<DomainLookupResult>
    {
        public void Configure(EntityTypeBuilder<DomainLookupResult> builder)
        {
            builder.ToTable("DomainLookupResults");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.DomainId)
                .IsRequired();

            builder.Property(l => l.ProviderId)
                .IsRequired();

            builder.Property(l => l.IsAvailable)
                .HasDefaultValue(false);

            builder.Property(l => l.ResponseTimeMs)
                .HasDefaultValue(0);

            builder.Property(l => l.RawResponse)
                .HasColumnType("text");

            builder.Property(l => l.CheckedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasOne(l => l.Domain)
                .WithMany(d => d.LookupResults)
                .HasForeignKey(l => l.DomainId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.Provider)
                .WithMany(p => p.LookupResults)
                .HasForeignKey(l => l.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
