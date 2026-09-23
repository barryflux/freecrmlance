using Freecrmlance.Domain.Platform;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Platform;

public sealed class WorkspacePersistenceTests
{
    [Test]
    public async Task Workspace_and_owner_membership_are_persisted_on_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Freelance");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "identity-user-id", WorkspaceRole.Owner));
        await db.SaveChangesAsync();

        var member = await db.WorkspaceMembers.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(member.WorkspaceId, Is.EqualTo(workspace.Id));
            Assert.That(member.UserId, Is.EqualTo("identity-user-id"));
            Assert.That(member.Role, Is.EqualTo(WorkspaceRole.Owner));
        });
    }

    [Test]
    public async Task Duplicate_membership_is_rejected_by_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Freelance");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "identity-user-id", WorkspaceRole.Owner));
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "identity-user-id", WorkspaceRole.Owner));

        Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }
}
