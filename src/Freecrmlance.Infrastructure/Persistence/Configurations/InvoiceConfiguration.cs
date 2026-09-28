using Freecrmlance.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", "billing");
        builder.HasKey(invoice => invoice.Id);
        builder.Property(invoice => invoice.DraftReference).HasMaxLength(50).IsRequired();
        builder.Property(invoice => invoice.Number).HasMaxLength(50);
        builder.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(invoice => invoice.CreatedAtUtc).IsRequired();
        builder.Property(invoice => invoice.UpdatedAtUtc).IsRequired();
        builder.Property(invoice => invoice.PurchaseOrderReference).HasMaxLength(100);
        builder.Property(invoice => invoice.VatExemptionMention).HasMaxLength(500);
        builder.Property(invoice => invoice.PaymentTerms).HasMaxLength(500);
        builder.Property(invoice => invoice.EarlyPaymentDiscountTerms).HasMaxLength(500);
        builder.Property(invoice => invoice.LatePaymentPenaltyTerms).HasMaxLength(500);
        builder.Property(invoice => invoice.RecoveryCostIndemnity).HasPrecision(18, 2);
        builder.Property(invoice => invoice.SellerLegalName).HasMaxLength(200);
        builder.Property(invoice => invoice.SellerLegalForm).HasMaxLength(100);
        builder.Property(invoice => invoice.SellerSiren).HasMaxLength(9);
        builder.Property(invoice => invoice.SellerSiret).HasMaxLength(14);
        builder.Property(invoice => invoice.SellerVatNumber).HasMaxLength(30);
        builder.Property(invoice => invoice.SellerAddressLine1).HasMaxLength(200);
        builder.Property(invoice => invoice.SellerAddressLine2).HasMaxLength(200);
        builder.Property(invoice => invoice.SellerPostalCode).HasMaxLength(20);
        builder.Property(invoice => invoice.SellerCity).HasMaxLength(100);
        builder.Property(invoice => invoice.SellerCountryCode).HasMaxLength(2);
        builder.Property(invoice => invoice.SellerContactEmail).HasMaxLength(320);
        builder.Property(invoice => invoice.SellerContactPhone).HasMaxLength(50);
        builder.Property(invoice => invoice.CustomerLegalName).HasMaxLength(200);
        builder.Property(invoice => invoice.CustomerSiren).HasMaxLength(9);
        builder.Property(invoice => invoice.CustomerSiret).HasMaxLength(14);
        builder.Property(invoice => invoice.CustomerVatNumber).HasMaxLength(30);
        builder.Property(invoice => invoice.CustomerAddressLine1).HasMaxLength(200);
        builder.Property(invoice => invoice.CustomerAddressLine2).HasMaxLength(200);
        builder.Property(invoice => invoice.CustomerPostalCode).HasMaxLength(20);
        builder.Property(invoice => invoice.CustomerCity).HasMaxLength(100);
        builder.Property(invoice => invoice.CustomerCountryCode).HasMaxLength(2);
        builder.Ignore(invoice => invoice.Total);
        builder.Ignore(invoice => invoice.TotalExcludingTax);
        builder.Ignore(invoice => invoice.TotalVat);
        builder.Ignore(invoice => invoice.TotalIncludingTax);
        builder.HasIndex(invoice => new { invoice.WorkspaceId, invoice.Number }).IsUnique().HasFilter("\"Number\" IS NOT NULL");
        builder.HasIndex(invoice => new { invoice.WorkspaceId, invoice.CustomerId });
        builder.HasIndex(invoice => invoice.SourceQuoteId).IsUnique();
        builder.HasMany(invoice => invoice.Lines).WithOne().HasForeignKey(line => line.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(invoice => invoice.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
