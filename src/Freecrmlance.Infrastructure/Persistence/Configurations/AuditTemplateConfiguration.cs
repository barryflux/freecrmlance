using Freecrmlance.Domain.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditTemplateConfiguration : IEntityTypeConfiguration<AuditTemplate>
{
 public void Configure(EntityTypeBuilder<AuditTemplate> b)
 {
  b.ToTable("AuditTemplates","audit"); b.HasKey(x=>x.Id);
  b.Property(x=>x.Name).HasMaxLength(200).IsRequired(); b.Property(x=>x.Description).HasMaxLength(2000);
  b.HasIndex(x=>new{x.WorkspaceId,x.IsArchived});
  b.HasMany(x=>x.Sections).WithOne().HasForeignKey(x=>x.AuditTemplateId).OnDelete(DeleteBehavior.Cascade);
  b.Navigation(x=>x.Sections).UsePropertyAccessMode(PropertyAccessMode.Field);
 }
}