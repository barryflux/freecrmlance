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


    [Test]
    public async Task Create_persists_customer_and_contacts_in_one_save()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceId = Guid.NewGuid();
        var service = new CustomerService(db, new StubWorkspaceContext(workspaceId));

        var id = await service.CreateAsync(new CreateCustomerCommand(
            "Acme",
            "hello@acme.test",
            null,
            [
                new CreateContactCommand("Alice", "alice@acme.test", null, "CEO"),
                new CreateContactCommand("Bob", null, "0102030405", null)
            ]));

        var customer = await db.Customers.SingleAsync(customer => customer.Id == id);
        var contacts = await db.Contacts.Where(contact => contact.CustomerId == id).OrderBy(contact => contact.Name).ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(customer.WorkspaceId, Is.EqualTo(workspaceId));
            Assert.That(contacts.Select(contact => contact.Name), Is.EqualTo(new[] { "Alice", "Bob" }));
            Assert.That(contacts.All(contact => contact.WorkspaceId == workspaceId), Is.True);
        });
    }

    [Test]
    public async Task Create_does_not_persist_customer_when_an_initial_contact_is_invalid()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).Options;

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceId = Guid.NewGuid();
        var service = new CustomerService(db, new StubWorkspaceContext(workspaceId));

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.CreateAsync(new CreateCustomerCommand(
                "Should not exist",
                null,
                null,
                [
                    new CreateContactCommand("Valid contact", null, null, null),
                    new CreateContactCommand(" ", null, null, null)
                ])));

        Assert.Multiple(() =>
        {
            Assert.That(db.Customers.Count(), Is.EqualTo(0));
            Assert.That(db.Contacts.Count(), Is.EqualTo(0));
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
