using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainVisitConfiguration : IEntityTypeConfiguration<DomainVisit>
    {
        public void Configure(EntityTypeBuilder<DomainVisit> builder)
        {
            builder.ToTable("DomainVisits");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.DomainId)
                .IsRequired();

            builder.Property(v => v.ProviderId)
                .IsRequired();

            builder.Property(v => v.VisitLink)
                .HasMaxLength(500);

            builder.Property(v => v.VisitedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasOne(v => v.Domain)
                .WithMany(d => d.Visits)
                .HasForeignKey(v => v.DomainId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Provider)
                .WithMany(p => p.Visits)
                .HasForeignKey(v => v.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
