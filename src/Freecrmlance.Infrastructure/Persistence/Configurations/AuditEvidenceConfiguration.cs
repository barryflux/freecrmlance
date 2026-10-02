using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class AuditEvidenceConfiguration : IEntityTypeConfiguration<AuditEvidence>
{
    public void Configure(EntityTypeBuilder<AuditEvidence> builder)
    {
        builder.ToTable("AuditEvidence", "audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UploadedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => x.AuditItemId);
        builder.HasIndex(x => x.DocumentId).IsUnique();
        builder.HasOne<AuditItem>().WithMany().HasForeignKey(x => x.AuditItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Document>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
    }
}
