using Freecrmlance.Domain.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freecrmlance.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("WorkspaceMembers", "platform");
        builder.HasKey(member => new { member.WorkspaceId, member.UserId });
        builder.Property(member => member.UserId).HasMaxLength(450).IsRequired();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasOne(member => member.Workspace)
            .WithMany()
            .HasForeignKey(member => member.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
