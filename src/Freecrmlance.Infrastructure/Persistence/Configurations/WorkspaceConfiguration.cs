using Freecrmlance.Domain.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces", "platform");
        builder.HasKey(workspace => workspace.Id);
        builder.Property(workspace => workspace.Name).HasMaxLength(200).IsRequired();
        builder.Property(workspace => workspace.LegalName).HasMaxLength(200);
        builder.Property(workspace => workspace.LegalForm).HasMaxLength(100);
        builder.Property(workspace => workspace.Siren).HasMaxLength(9);
        builder.Property(workspace => workspace.Siret).HasMaxLength(14);
        builder.Property(workspace => workspace.VatNumber).HasMaxLength(30);
        builder.Property(workspace => workspace.AddressLine1).HasMaxLength(200);
        builder.Property(workspace => workspace.AddressLine2).HasMaxLength(200);
        builder.Property(workspace => workspace.PostalCode).HasMaxLength(20);
        builder.Property(workspace => workspace.City).HasMaxLength(100);
        builder.Property(workspace => workspace.CountryCode).HasMaxLength(2);
        builder.Property(workspace => workspace.ContactEmail).HasMaxLength(320);
        builder.Property(workspace => workspace.ContactPhone).HasMaxLength(50);
    }
}
