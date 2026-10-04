using Freecrmlance.Domain.Audits;using Freecrmlance.Domain.Crm;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditReportTransmissionConfiguration:IEntityTypeConfiguration<AuditReportTransmission>
{
 public void Configure(EntityTypeBuilder<AuditReportTransmission>b){b.ToTable("AuditReportTransmissions","audit");b.HasKey(x=>x.Id);b.Property(x=>x.Recipient).HasMaxLength(320).IsRequired();b.Property(x=>x.SentByUserId).HasMaxLength(450).IsRequired();b.HasIndex(x=>new{x.WorkspaceId,x.AuditId});b.HasOne<Audit>().WithMany().HasForeignKey(x=>x.AuditId).OnDelete(DeleteBehavior.Cascade);b.HasOne<AuditReportVersion>().WithMany().HasForeignKey(x=>x.ReportVersionId).OnDelete(DeleteBehavior.Cascade);b.HasOne<DocumentShare>().WithMany().HasForeignKey(x=>x.DocumentShareId).OnDelete(DeleteBehavior.Restrict);}
}
