using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class AuditReportVersionConfiguration : IEntityTypeConfiguration<AuditReportVersion>
{
    public void Configure(EntityTypeBuilder<AuditReportVersion> builder)
    {
        builder.ToTable("AuditReportVersions", "audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FinalizedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Snapshot).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Hash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.AuditId, x.VersionNumber }).IsUnique();
        builder.HasOne<Audit>().WithMany().HasForeignKey(x => x.AuditId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Document>().WithMany().HasForeignKey(x => x.GeneratedDocumentId).OnDelete(DeleteBehavior.SetNull);
    }
}
