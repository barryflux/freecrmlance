using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class ContactServiceTests
{
    [Test]
    public async Task Contacts_are_created_and_listed_only_for_current_workspace_customer()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var ownCustomer = new Customer(workspaceA, "Acme");
        var otherCustomer = new Customer(workspaceB, "Wayne Enterprises");
        db.Customers.AddRange(ownCustomer, otherCustomer);
        await db.SaveChangesAsync();

        var service = new ContactService(db, new StubWorkspaceContext(workspaceA));
        var created = await service.CreateAsync(ownCustomer.Id, new CreateContactCommand("Alice", "alice@example.test", "123", "Buyer"));
        var crossWorkspace = await service.CreateAsync(otherCustomer.Id, new CreateContactCommand("Bruce", null, null, null));
        var ownContacts = await service.ListAsync(ownCustomer.Id);
        var hiddenContacts = await service.ListAsync(otherCustomer.Id);

        Assert.Multiple(() =>
        {
            Assert.That(created, Is.Not.Null);
            Assert.That(crossWorkspace, Is.Null);
            Assert.That(ownContacts, Has.Count.EqualTo(1));
            Assert.That(ownContacts![0].Name, Is.EqualTo("Alice"));
            Assert.That(hiddenContacts, Is.Null);
        });
    }

    [Test]
    public async Task Contact_update_is_scoped_by_workspace_customer_and_contact()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var customerA = new Customer(workspaceA, "Acme");
        var secondCustomerA = new Customer(workspaceA, "Globex");
        var customerB = new Customer(workspaceB, "Wayne");
        db.Customers.AddRange(customerA, secondCustomerA, customerB);
        await db.SaveChangesAsync();

        var contactA = new Contact(workspaceA, customerA.Id, "Alice");
        var contactB = new Contact(workspaceB, customerB.Id, "Bruce");
        db.Contacts.AddRange(contactA, contactB);
        await db.SaveChangesAsync();

        var service = new ContactService(db, new StubWorkspaceContext(workspaceA));

        var own = await service.UpdateAsync(customerA.Id, contactA.Id, new UpdateContactCommand("Alice Smith", "alice@example.test", "456", "CTO"));
        var wrongCustomer = await service.UpdateAsync(secondCustomerA.Id, contactA.Id, new UpdateContactCommand("Hidden", null, null, null));
        var crossWorkspace = await service.UpdateAsync(customerB.Id, contactB.Id, new UpdateContactCommand("Hidden", null, null, null));
        var updated = await service.GetAsync(customerA.Id, contactA.Id);
        var hidden = await service.GetAsync(customerB.Id, contactB.Id);

        Assert.Multiple(() =>
        {
            Assert.That(own, Is.True);
            Assert.That(wrongCustomer, Is.False);
            Assert.That(crossWorkspace, Is.False);
            Assert.That(updated!.Name, Is.EqualTo("Alice Smith"));
            Assert.That(updated.Role, Is.EqualTo("CTO"));
            Assert.That(hidden, Is.Null);
        });
    }

    [Test]
    public async Task Contact_delete_is_scoped_by_workspace_customer_and_contact()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var customerA = new Customer(workspaceA, "Acme");
        var secondCustomerA = new Customer(workspaceA, "Globex");
        var customerB = new Customer(workspaceB, "Wayne");
        db.Customers.AddRange(customerA, secondCustomerA, customerB);
        await db.SaveChangesAsync();

        var contactA = new Contact(workspaceA, customerA.Id, "Alice");
        var contactB = new Contact(workspaceB, customerB.Id, "Bruce");
        db.Contacts.AddRange(contactA, contactB);
        await db.SaveChangesAsync();

        var service = new ContactService(db, new StubWorkspaceContext(workspaceA));

        var wrongCustomer = await service.DeleteAsync(secondCustomerA.Id, contactA.Id);
        var crossWorkspace = await service.DeleteAsync(customerB.Id, contactB.Id);
        var own = await service.DeleteAsync(customerA.Id, contactA.Id);
        var deleted = await service.GetAsync(customerA.Id, contactA.Id);
        var otherStillExists = await db.Contacts.AsNoTracking().AnyAsync(contact => contact.Id == contactB.Id);

        Assert.Multiple(() =>
        {
            Assert.That(wrongCustomer, Is.False);
            Assert.That(crossWorkspace, Is.False);
            Assert.That(own, Is.True);
            Assert.That(deleted, Is.Null);
            Assert.That(otherStillExists, Is.True);
        });
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
