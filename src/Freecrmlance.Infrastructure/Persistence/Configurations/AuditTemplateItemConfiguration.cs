using Freecrmlance.Domain.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditTemplateItemConfiguration : IEntityTypeConfiguration<AuditTemplateItem>
{
 public void Configure(EntityTypeBuilder<AuditTemplateItem> b)
 {
  b.ToTable("AuditTemplateItems","audit"); b.HasKey(x=>x.Id);
  b.Property(x=>x.Label).HasMaxLength(500).IsRequired(); b.Property(x=>x.Description).HasMaxLength(2000);
  b.Property(x=>x.ResponseType).HasConversion<string>().HasMaxLength(32).IsRequired(); b.Property(x=>x.Options).HasMaxLength(4000);
  b.HasIndex(x=>new{x.SectionId,x.Position}).IsUnique();
 }
}