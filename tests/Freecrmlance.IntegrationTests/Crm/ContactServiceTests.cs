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

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
