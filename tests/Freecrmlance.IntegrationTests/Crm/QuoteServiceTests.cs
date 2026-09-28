using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class QuoteServiceTests
{
    [Test]
    public async Task Create_and_read_are_scoped_to_current_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var customerA = new Customer(workspaceA, "Acme");
        var customerB = new Customer(workspaceB, "Wayne");
        db.Customers.AddRange(customerA, customerB);
        await db.SaveChangesAsync();

        var serviceA = new QuoteService(db, new StubWorkspaceContext(workspaceA));
        var serviceB = new QuoteService(db, new StubWorkspaceContext(workspaceB));

        var quoteId = await serviceA.CreateAsync(new CreateQuoteCommand(
            customerA.Id,
            "Q-001",
            [new CreateQuoteLineCommand("Design", 2, 150m), new CreateQuoteLineCommand("Hosting", 1, 50m)]));

        var own = await serviceA.GetAsync(customerA.Id, quoteId!.Value);
        var crossWorkspace = await serviceB.GetAsync(customerA.Id, quoteId.Value);
        var crossWorkspaceCreate = await serviceB.CreateAsync(new CreateQuoteCommand(customerA.Id, "Q-002"));

        Assert.Multiple(() =>
        {
            Assert.That(own, Is.Not.Null);
            Assert.That(own!.Total, Is.EqualTo(350m));
            Assert.That(own.Lines, Has.Count.EqualTo(2));
            Assert.That(crossWorkspace, Is.Null);
            Assert.That(crossWorkspaceCreate, Is.Null);
        });
    }

    [Test]
    public async Task UpdateDraft_replaces_lines_and_remains_tenant_scoped()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var customer = new Customer(workspaceA, "Acme");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var serviceA = new QuoteService(db, new StubWorkspaceContext(workspaceA));
        var serviceB = new QuoteService(db, new StubWorkspaceContext(workspaceB));
        var quoteId = (await serviceA.CreateAsync(new CreateQuoteCommand(
            customer.Id, "Q-001", [new CreateQuoteLineCommand("Old", 1, 10)])))!.Value;

        // Simulate the next HTTP request: production uses a fresh scoped DbContext.
        db.ChangeTracker.Clear();

        var crossWorkspace = await serviceB.UpdateDraftAsync(
            customer.Id, quoteId, new UpdateQuoteCommand("HACK", []));
        var updated = await serviceA.UpdateDraftAsync(
            customer.Id, quoteId, new UpdateQuoteCommand(
                "Q-002",
                [new CreateQuoteLineCommand("Design", 2, 150), new CreateQuoteLineCommand("Hosting", 1, 50)]));

        db.ChangeTracker.Clear();
        var quote = await serviceA.GetAsync(customer.Id, quoteId);

        Assert.Multiple(() =>
        {
            Assert.That(crossWorkspace, Is.False);
            Assert.That(updated, Is.True);
            Assert.That(quote!.Number, Is.EqualTo("Q-002"));
            Assert.That(quote.Lines.Select(line => line.Description), Is.EquivalentTo(new[] { "Design", "Hosting" }));
            Assert.That(quote.Total, Is.EqualTo(350m));
        });
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
