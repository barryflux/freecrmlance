using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class CustomerServiceTests
{
    [Test]
    public async Task Customers_are_scoped_to_current_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        db.Customers.AddRange(
            new Customer(workspaceA, "Visible"),
            new Customer(workspaceB, "Hidden"));
        await db.SaveChangesAsync();

        var service = new CustomerService(db, new StubWorkspaceContext(workspaceA));
        var customers = await service.ListAsync();

        Assert.That(customers.Select(customer => customer.Name), Is.EqualTo(new[] { "Visible" }));
    }

    [Test]
    public async Task Create_uses_current_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceId = Guid.NewGuid();
        var service = new CustomerService(db, new StubWorkspaceContext(workspaceId));

        var id = await service.CreateAsync(new CreateCustomerCommand("Acme", "hello@acme.test", null));

        var customer = await db.Customers.SingleAsync(customer => customer.Id == id);
        Assert.That(customer.WorkspaceId, Is.EqualTo(workspaceId));
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(workspaceId);

        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(workspaceId);
    }
}
