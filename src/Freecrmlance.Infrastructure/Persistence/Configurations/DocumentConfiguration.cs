using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents", "crm");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.FileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(255).IsRequired();
        builder.Property(document => document.StorageKey).HasMaxLength(200).IsRequired();
        builder.Property(document => document.CreatedAtUtc).IsRequired();
        builder.HasIndex(document => new { document.WorkspaceId, document.CustomerId });
        builder.HasIndex(document => document.StorageKey).IsUnique();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(document => document.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
