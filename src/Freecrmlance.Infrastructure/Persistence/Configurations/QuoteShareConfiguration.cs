using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class QuoteShareConfiguration : IEntityTypeConfiguration<QuoteShare>
{
    public void Configure(EntityTypeBuilder<QuoteShare> builder)
    {
        builder.ToTable("QuoteShares", "crm");
        builder.HasKey(share => share.Id);
        builder.Property(share => share.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(share => share.CreatedAtUtc).IsRequired();
        builder.HasIndex(share => share.TokenHash).IsUnique();
        builder.HasIndex(share => new { share.WorkspaceId, share.CustomerId, share.QuoteId });

        builder.HasOne<Quote>()
            .WithMany()
            .HasForeignKey(share => share.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
