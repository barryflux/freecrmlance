using Freecrmlance.Domain.Audits;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditHistoryEventConfiguration:IEntityTypeConfiguration<AuditHistoryEvent>
{
 public void Configure(EntityTypeBuilder<AuditHistoryEvent>b){b.ToTable("AuditHistoryEvents","audit");b.HasKey(x=>x.Id);b.Property(x=>x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();b.Property(x=>x.UserId).HasMaxLength(450);b.HasIndex(x=>new{x.WorkspaceId,x.AuditId,x.OccurredAtUtc});b.HasOne<Audit>().WithMany().HasForeignKey(x=>x.AuditId).OnDelete(DeleteBehavior.Cascade);b.HasOne<AuditReportVersion>().WithMany().HasForeignKey(x=>x.ReportVersionId).OnDelete(DeleteBehavior.Cascade);}
}
