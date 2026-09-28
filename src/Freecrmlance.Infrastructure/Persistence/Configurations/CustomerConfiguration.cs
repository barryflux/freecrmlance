using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "crm");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(320);
        builder.Property(customer => customer.Phone).HasMaxLength(50);
        builder.Property(customer => customer.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(customer => customer.Siren).HasMaxLength(9);
        builder.Property(customer => customer.Siret).HasMaxLength(14);
        builder.Property(customer => customer.VatNumber).HasMaxLength(30);
        builder.Property(customer => customer.AddressLine1).HasMaxLength(200);
        builder.Property(customer => customer.AddressLine2).HasMaxLength(200);
        builder.Property(customer => customer.PostalCode).HasMaxLength(20);
        builder.Property(customer => customer.City).HasMaxLength(100);
        builder.Property(customer => customer.CountryCode).HasMaxLength(2);
        builder.Property(customer => customer.BillingAddressLine1).HasMaxLength(200);
        builder.Property(customer => customer.BillingAddressLine2).HasMaxLength(200);
        builder.Property(customer => customer.BillingPostalCode).HasMaxLength(20);
        builder.Property(customer => customer.BillingCity).HasMaxLength(100);
        builder.Property(customer => customer.BillingCountryCode).HasMaxLength(2);
        builder.HasIndex(customer => customer.WorkspaceId);
    }
}
