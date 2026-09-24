using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts", "crm");
        builder.HasKey(contact => contact.Id);
        builder.Property(contact => contact.Name).HasMaxLength(200).IsRequired();
        builder.Property(contact => contact.Email).HasMaxLength(320);
        builder.Property(contact => contact.Phone).HasMaxLength(50);
        builder.Property(contact => contact.Role).HasMaxLength(100);
        builder.HasIndex(contact => new { contact.WorkspaceId, contact.CustomerId });
        builder.HasOne<Customer>().WithMany().HasForeignKey(contact => contact.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
