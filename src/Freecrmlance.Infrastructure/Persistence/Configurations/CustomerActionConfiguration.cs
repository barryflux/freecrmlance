using Freecrmlance.Domain.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class CustomerActionConfiguration : IEntityTypeConfiguration<CustomerAction>
{
    public void Configure(EntityTypeBuilder<CustomerAction> builder)
    {
        builder.ToTable("CustomerActions", "crm");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.HasIndex(x => new { x.WorkspaceId, x.CustomerId, x.Status });
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
