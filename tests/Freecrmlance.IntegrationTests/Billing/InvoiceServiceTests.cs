using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Billing;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Domain.Platform;
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

        var workspaceA = new Workspace("Workspace A");
        var workspaceB = new Workspace("Workspace B");
        var customer = new Customer(workspaceA.Id, "Acme");
        db.Workspaces.AddRange(workspaceA, workspaceB);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var serviceA = new InvoiceService(db, new StubWorkspaceContext(workspaceA.Id));
        var serviceB = new InvoiceService(db, new StubWorkspaceContext(workspaceB.Id));
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

        var workspace = new Workspace("Seller workspace");
        workspace.SetLegalIdentity("Seller SAS", "1 rue Seller", "75001", "Paris", "FR", siren: "123456789");
        var customer = new Customer(workspace.Id, "Acme");
        var accepted = new Quote(workspace.Id, customer.Id, "Q-001");
        accepted.AddLine("Design", 2, 150m);
        accepted.MarkSent();
        accepted.Accept();
        var draft = new Quote(workspace.Id, customer.Id, "Q-002");
        db.Workspaces.Add(workspace);
        db.Customers.Add(customer);
        db.Quotes.AddRange(accepted, draft);
        await db.SaveChangesAsync();

        var service = new InvoiceService(db, new StubWorkspaceContext(workspace.Id));
        var invoiceId = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, accepted.Id, "INV-001"));
        var duplicate = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, accepted.Id, "INV-002"));
        var invalidStatus = await service.CreateFromAcceptedQuoteAsync(new(customer.Id, draft.Id, "INV-003"));

        db.ChangeTracker.Clear();
        var invoice = await service.GetAsync(customer.Id, invoiceId!.Value);

        Assert.Multiple(() =>
        {
            Assert.That(invoice!.SourceQuoteId, Is.EqualTo(accepted.Id));
            Assert.That(invoice.SellerLegalName, Is.EqualTo("Seller SAS"));
            Assert.That(invoice.SellerSiren, Is.EqualTo("123456789"));
            Assert.That(invoice.CustomerLegalName, Is.EqualTo("Acme"));
            Assert.That(invoice.Total, Is.EqualTo(300m));
            Assert.That(invoice.Lines.Single().Description, Is.EqualTo("Design"));
            Assert.That(duplicate, Is.Null);
            Assert.That(invalidStatus, Is.Null);
        });
    }


    [Test]
    public async Task Complete_invoices_receive_sequential_numbers_per_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Seller");
        workspace.SetLegalIdentity("Seller SAS", "1 rue Seller", "75001", "Paris", "FR");
        var customer = new Customer(workspace.Id, "Customer");
        customer.SetBillingIdentity(CustomerType.Business, "2 rue Customer", "69001", "Lyon", "FR", siren: "123456789");
        db.Workspaces.Add(workspace);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var service = new InvoiceService(db, new StubWorkspaceContext(workspace.Id));
        var command1 = new CreateInvoiceCommand(customer.Id, "DRAFT-A", [new("Work", 1, 100m, 20m)],
            DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30), PaymentTerms: "30 days");
        var command2 = command1 with { Number = "DRAFT-B" };
        var firstId = (await service.CreateAsync(command1))!.Value;
        var secondId = (await service.CreateAsync(command2))!.Value;

        Assert.That(await service.IssueAsync(customer.Id, firstId), Is.EqualTo(IssueInvoiceResult.Success));
        Assert.That(await service.IssueAsync(customer.Id, secondId), Is.EqualTo(IssueInvoiceResult.Success));

        db.ChangeTracker.Clear();
        var first = await service.GetAsync(customer.Id, firstId);
        var second = await service.GetAsync(customer.Id, secondId);

        Assert.Multiple(() =>
        {
            Assert.That(first!.Status, Is.EqualTo(InvoiceStatus.Issued));
            Assert.That(first.IssueDate, Is.Not.Null);
            Assert.That(first.Number, Does.Match(@"^\\d{4}-0001$"));
            Assert.That(second!.Number, Does.Match(@"^\\d{4}-0002$"));
        });
    }

    [Test]
    public async Task Incomplete_invoice_is_not_issued()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();

        var workspace = new Workspace("Seller");
        var customer = new Customer(workspace.Id, "Customer");
        db.Workspaces.Add(workspace);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var service = new InvoiceService(db, new StubWorkspaceContext(workspace.Id));
        var invoiceId = (await service.CreateAsync(new CreateInvoiceCommand(customer.Id, "DRAFT-A", [new("Work", 1, 100m, 20m)])))!.Value;

        Assert.That(await service.IssueAsync(customer.Id, invoiceId), Is.EqualTo(IssueInvoiceResult.Incomplete));
        db.ChangeTracker.Clear();
        var invoice = await service.GetAsync(customer.Id, invoiceId);
        Assert.That(invoice!.Status, Is.EqualTo(InvoiceStatus.Draft));
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
