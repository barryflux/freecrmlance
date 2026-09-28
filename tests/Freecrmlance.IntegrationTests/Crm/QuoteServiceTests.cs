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

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }
}
