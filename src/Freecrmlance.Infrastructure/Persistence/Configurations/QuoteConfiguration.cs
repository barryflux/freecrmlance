using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes", "crm");
        builder.HasKey(quote => quote.Id);
        builder.Property(quote => quote.Number).HasMaxLength(50).IsRequired();
        builder.Property(quote => quote.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(quote => quote.CreatedAtUtc).IsRequired();
        builder.Property(quote => quote.UpdatedAtUtc).IsRequired();
        builder.HasIndex(quote => new { quote.WorkspaceId, quote.Number }).IsUnique();
        builder.HasIndex(quote => new { quote.WorkspaceId, quote.CustomerId });
        builder.HasMany(quote => quote.Lines).WithOne().HasForeignKey(line => line.QuoteId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(quote => quote.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
