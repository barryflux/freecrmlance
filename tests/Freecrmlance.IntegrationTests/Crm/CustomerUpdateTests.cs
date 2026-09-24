using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class CustomerUpdateTests
{
    [Test]
    public async Task Current_workspace_customer_can_be_updated_but_other_workspace_customer_cannot()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var own = new Customer(workspaceA, "Own");
        var other = new Customer(workspaceB, "Other");
        db.Customers.AddRange(own, other);
        await db.SaveChangesAsync();

        var service = new CustomerService(db, new StubWorkspaceContext(workspaceA));

        var updated = await service.UpdateAsync(own.Id, new UpdateCustomerCommand("Updated", "updated@example.test", "123"));
        var crossWorkspaceUpdated = await service.UpdateAsync(other.Id, new UpdateCustomerCommand("Hacked", null, null));

        await db.Entry(own).ReloadAsync();
        await db.Entry(other).ReloadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.True);
            Assert.That(own.Name, Is.EqualTo("Updated"));
            Assert.That(own.Email, Is.EqualTo("updated@example.test"));
            Assert.That(crossWorkspaceUpdated, Is.False);
            Assert.That(other.Name, Is.EqualTo("Other"));
        });
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(workspaceId);

        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(workspaceId);
    }
}
