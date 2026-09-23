using Freecrmlance.Domain.Platform;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Persistence;

public sealed class FreecrmlanceDbContext(DbContextOptions<FreecrmlanceDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(FreecrmlanceDbContext).Assembly);
    }
}
