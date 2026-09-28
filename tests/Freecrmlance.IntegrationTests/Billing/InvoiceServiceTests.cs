using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Billing;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Billing;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Billing;

public sealed class InvoiceServiceTests
{
    [Test]
    public async Task Create_and_read_are_tenant_scoped()
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

        var serviceA = new InvoiceService(db, new StubWorkspaceContext(workspaceA));
        var serviceB = new InvoiceService(db, new StubWorkspaceContext(workspaceB));
        var id = await serviceA.CreateAsync(new CreateInvoiceCommand(customer.Id, "INV-001", [new("Design", 2, 150m)]));

        db.ChangeTracker.Clear();
        var own = await serviceA.GetAsync(customer.Id, id!.Value);
        var crossWorkspace = await serviceB.GetAsync(customer.Id, id.Value);

        Assert.Multiple(() =>
        {
            Assert.That(own!.Total, Is.EqualTo(300m));
            Assert.That(own.Status, Is.EqualTo(InvoiceStatus.Draft));
            Assert.That(crossWorkspace, Is.Null);
        });
    }

    [Test]
    public async Task Accepted_quote_is_snapshotted_once_into_invoice()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspaceId = Guid.NewGuid();
        var customer = new Customer(workspaceId, "Acme");
        var accepted = new Quote(workspaceId, customer.Id, "Q-001");
        accepted.AddLine("Design", 2, 150m);
        accepted.MarkSent();
        accepted.Accept();
        var draft = new Quote(workspaceId, customer.Id, "Q-002");
        db.Customers.Add(customer);
        db.Quotes.AddRange(accepted, draft);
        await db.SaveChangesAsync();

        var service = new InvoiceService(db, new StubWorkspaceContext(workspaceId));
        var invoiceId = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, accepted.Id, "INV-001"));
        var duplicate = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, accepted.Id, "INV-002"));
        var invalidStatus = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, draft.Id, "INV-003"));

        db.ChangeTracker.Clear();
        var invoice = await service.GetAsync(customer.Id, invoiceId!.Value);

        Assert.Multiple(() =>
        {
            Assert.That(invoice!.SourceQuoteId, Is.EqualTo(accepted.Id));
            Assert.That(invoice.Total, Is.EqualTo(300m));
            Assert.That(invoice.Lines.Single().Description, Is.EqualTo("Design"));
            Assert.That(duplicate, Is.Null);
            Assert.That(invalidStatus, Is.Null);
        });
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
