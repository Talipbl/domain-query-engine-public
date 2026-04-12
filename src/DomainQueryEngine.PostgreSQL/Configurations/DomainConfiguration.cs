using DomainQueryEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DomainQueryEngine.PostgreSQL
{
    public class DomainConfiguration : IEntityTypeConfiguration<Domain>
    {
        public void Configure(EntityTypeBuilder<Domain> builder)
        {
            builder.ToTable("Domains");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DomainName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Tld)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasMany(d => d.LookupResults)
                .WithOne(l => l.Domain)
                .HasForeignKey(l => l.DomainId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Visits)
                .WithOne(v => v.Domain)
                .HasForeignKey(v => v.DomainId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
