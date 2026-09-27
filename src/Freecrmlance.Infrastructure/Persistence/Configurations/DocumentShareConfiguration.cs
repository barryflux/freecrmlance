using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class DocumentShareConfiguration : IEntityTypeConfiguration<DocumentShare>
{
    public void Configure(EntityTypeBuilder<DocumentShare> builder)
    {
        builder.ToTable("DocumentShares", "crm");
        builder.HasKey(share => share.Id);
        builder.Property(share => share.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(share => share.CreatedAtUtc).IsRequired();
        builder.HasIndex(share => share.TokenHash).IsUnique();
        builder.HasIndex(share => new { share.WorkspaceId, share.CustomerId, share.DocumentId });

        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(share => share.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
