using Freecrmlance.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines", "billing");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Description).HasMaxLength(500).IsRequired();
        builder.Property(line => line.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(line => line.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(line => line.VatRate).HasPrecision(5, 2).IsRequired();
        builder.Ignore(line => line.Total);
        builder.Ignore(line => line.TotalExcludingTax);
        builder.Ignore(line => line.VatAmount);
        builder.Ignore(line => line.TotalIncludingTax);
    }
}
