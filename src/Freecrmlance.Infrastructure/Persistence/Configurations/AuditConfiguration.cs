using Freecrmlance.Domain.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Freecrmlance.Infrastructure.Persistence.Configurations;
public sealed class AuditConfiguration:IEntityTypeConfiguration<Audit>{public void Configure(EntityTypeBuilder<Audit>b){b.ToTable("Audits","audit");b.HasKey(x=>x.Id);b.Property(x=>x.Reference).HasMaxLength(32).IsRequired();b.Property(x=>x.Title).HasMaxLength(200).IsRequired();b.Property(x=>x.Description).HasMaxLength(2000);b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32);b.HasIndex(x=>new{x.WorkspaceId,x.Reference}).IsUnique();b.HasIndex(x=>new{x.WorkspaceId,x.CustomerId});b.HasMany(x=>x.Sections).WithOne().HasForeignKey(x=>x.AuditId).OnDelete(DeleteBehavior.Cascade);b.Navigation(x=>x.Sections).UsePropertyAccessMode(PropertyAccessMode.Field);}}
