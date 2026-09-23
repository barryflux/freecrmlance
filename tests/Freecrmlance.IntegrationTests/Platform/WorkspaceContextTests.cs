using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Platform;
using Freecrmlance.Infrastructure.Persistence;
using Freecrmlance.Infrastructure.Platform;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Platform;

public sealed class WorkspaceContextTests
{
    [Test]
    public async Task Member_resolves_own_workspace_from_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Mine");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "user-a", WorkspaceRole.Owner));
        await db.SaveChangesAsync();

        var context = new WorkspaceContext(new StubCurrentUser("user-a"), db);

        Assert.That(await context.GetCurrentWorkspaceIdAsync(), Is.EqualTo(workspace.Id));
    }

    [Test]
    public async Task User_without_membership_has_no_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Someone else's");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "user-b", WorkspaceRole.Owner));
        await db.SaveChangesAsync();

        var context = new WorkspaceContext(new StubCurrentUser("user-a"), db);

        Assert.That(await context.GetCurrentWorkspaceIdAsync(), Is.Null);
        Assert.ThrowsAsync<WorkspaceNotAvailableException>(
            async () => await context.RequireCurrentWorkspaceIdAsync());
    }

    private sealed class StubCurrentUser(string? userId) : ICurrentUser
    {
        public string? UserId { get; } = userId;
    }
}
