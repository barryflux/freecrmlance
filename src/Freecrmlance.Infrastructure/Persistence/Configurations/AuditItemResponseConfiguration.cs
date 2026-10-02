using Freecrmlance.Domain.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditItemResponseConfiguration:IEntityTypeConfiguration<AuditItemResponse>
{
 public void Configure(EntityTypeBuilder<AuditItemResponse>b)
 {
  b.ToTable("AuditItemResponses","audit");b.HasKey(x=>x.Id);
  b.Property(x=>x.Value).HasMaxLength(4000);b.Property(x=>x.Observation).HasMaxLength(4000);b.Property(x=>x.Recommendation).HasMaxLength(4000);
  b.Property(x=>x.UpdatedByUserId).HasMaxLength(450).IsRequired();
  b.HasIndex(x=>x.AuditItemId).IsUnique();
  b.HasOne<AuditItem>().WithOne().HasForeignKey<AuditItemResponse>(x=>x.AuditItemId).OnDelete(DeleteBehavior.Cascade);
 }
}
