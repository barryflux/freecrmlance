using Freecrmlance.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class InvoiceNumberSequenceConfiguration : IEntityTypeConfiguration<InvoiceNumberSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceNumberSequence> builder)
    {
        builder.ToTable("InvoiceNumberSequences", "billing");
        builder.HasKey(sequence => new { sequence.WorkspaceId, sequence.Year });
        builder.Property(sequence => sequence.LastNumber).IsRequired();
    }
}
