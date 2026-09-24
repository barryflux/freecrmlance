using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class CustomerDetailsTests
{
    [Test]
    public async Task Current_workspace_customer_is_returned_but_other_workspace_customer_is_not()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var visible = new Customer(workspaceA, "Visible", "visible@example.test", "0102030405");
        var hidden = new Customer(workspaceB, "Hidden");
        db.Customers.AddRange(visible, hidden);
        await db.SaveChangesAsync();

        var service = new CustomerService(db, new StubWorkspaceContext(workspaceA));

        var ownCustomer = await service.GetAsync(visible.Id);
        var otherCustomer = await service.GetAsync(hidden.Id);
        var missingCustomer = await service.GetAsync(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(ownCustomer?.Name, Is.EqualTo("Visible"));
            Assert.That(otherCustomer, Is.Null);
            Assert.That(missingCustomer, Is.Null);
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
