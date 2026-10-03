using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Audits;

public sealed class AuditFinalizationTests
{
    [Test]
    public async Task Required_item_must_be_answered_before_finalization()
    {
        await using var pg = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build(); await pg.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(pg.GetConnectionString()).Options;
        var wid = Guid.NewGuid();
        await using var db = new FreecrmlanceDbContext(options); await db.Database.MigrateAsync();
        var customer = new Customer(wid, "Client"); db.Customers.Add(customer);
        var audit = NewAudit(wid, customer.Id, true); db.Audits.Add(audit); await db.SaveChangesAsync();
        var service = new AuditFinalizationService(db, new Stub(wid));

        var result = await service.FinalizeAsync(audit.Id, "user");
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Error, Does.Contain("obligatoire"));
        Assert.That(await db.AuditReportVersions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Reopening_and_refinalizing_creates_v2_without_changing_v1()
    {
        await using var pg = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build(); await pg.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(pg.GetConnectionString()).Options;
        var wid = Guid.NewGuid();
        await using var db = new FreecrmlanceDbContext(options); await db.Database.MigrateAsync();
        var customer = new Customer(wid, "Client"); db.Customers.Add(customer);
        var audit = NewAudit(wid, customer.Id, true); db.Audits.Add(audit); await db.SaveChangesAsync();
        var item = audit.Sections.Single().Items.Single();
        db.AuditItemResponses.Add(new AuditItemResponse(item.Id, "première", "obs", "reco", "user")); await db.SaveChangesAsync();
        var service = new AuditFinalizationService(db, new Stub(wid));

        var first = await service.FinalizeAsync(audit.Id, "user");
        Assert.That(first.Succeeded, Is.True);
        var v1 = await db.AuditReportVersions.AsNoTracking().SingleAsync(x => x.Id == first.VersionId);
        var snapshot = v1.Snapshot; var hash = v1.Hash;
        Assert.That(await service.VerifyHashAsync(audit.Id, v1.Id), Is.True);

        Assert.That(await service.ReopenAsync(audit.Id), Is.Null);
        var response = await db.AuditItemResponses.SingleAsync(x => x.AuditItemId == item.Id);
        response.Update("deuxième", "obs 2", "reco 2", "user"); await db.SaveChangesAsync();
        var second = await service.FinalizeAsync(audit.Id, "user");
        Assert.That(second.Succeeded, Is.True);

        var versions = await db.AuditReportVersions.AsNoTracking().OrderBy(x => x.VersionNumber).ToListAsync();
        Assert.That(versions.Select(x => x.VersionNumber), Is.EqualTo(new[] { 1, 2 }));
        Assert.That(versions[0].Snapshot, Is.EqualTo(snapshot));
        Assert.That(versions[0].Hash, Is.EqualTo(hash));
        Assert.That(versions[1].Snapshot, Is.Not.EqualTo(snapshot));
        Assert.That(await service.VerifyHashAsync(audit.Id, versions[1].Id), Is.True);
    }

    [Test]
    public async Task Another_workspace_cannot_list_or_finalize_audit()
    {
        await using var pg = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build(); await pg.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(pg.GetConnectionString()).Options;
        var owner = Guid.NewGuid(); var foreign = Guid.NewGuid();
        await using var db = new FreecrmlanceDbContext(options); await db.Database.MigrateAsync();
        var customer = new Customer(owner, "Client"); db.Customers.Add(customer); var audit = NewAudit(owner, customer.Id, false); db.Audits.Add(audit); await db.SaveChangesAsync();
        var service = new AuditFinalizationService(db, new Stub(foreign));
        Assert.That(await service.ListVersionsAsync(audit.Id), Is.Null);
        Assert.That((await service.FinalizeAsync(audit.Id, "foreign")).Error, Is.EqualTo("Audit introuvable."));
    }

    private static Audit NewAudit(Guid wid, Guid customerId, bool required)
    {
        var audit = new Audit(wid, customerId, Guid.NewGuid(), Guid.NewGuid().ToString("N"), "Audit", null);
        audit.AddSection("Section", 0).AddItem("Critère", AuditResponseType.Text, required, 0);
        return audit;
    }

    private sealed class Stub(Guid id) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken ct = default) => Task.FromResult<Guid?>(id);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken ct = default) => Task.FromResult(id);
    }
}
