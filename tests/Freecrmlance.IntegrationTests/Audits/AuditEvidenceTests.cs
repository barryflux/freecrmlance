using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Audits;

public sealed class AuditEvidenceTests
{
    [Test]
    public async Task Evidence_can_be_uploaded_listed_downloaded_and_deleted()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        var wid = Guid.NewGuid();
        var storage = new MemoryStorage();

        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();
        var customer = new Customer(wid, "Client");
        db.Customers.Add(customer);
        var audit = NewAudit(wid, customer.Id);
        db.Audits.Add(audit);
        await db.SaveChangesAsync();

        var itemId = audit.Sections.Single().Items.Single().Id;
        var service = new AuditEvidenceService(db, new Stub(wid), storage);
        await using var upload = new MemoryStream("preuve"u8.ToArray());
        var error = await service.UploadAsync(audit.Id, itemId, "preuve.txt", "text/plain", upload.Length, upload, "user-1");
        Assert.That(error, Is.Null);

        var list = await service.ListAsync(audit.Id);
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].FileName, Is.EqualTo("preuve.txt"));
        Assert.That((await db.Audits.SingleAsync(x => x.Id == audit.Id)).Status, Is.EqualTo(AuditStatus.InProgress));

        await using var download = (await service.DownloadAsync(audit.Id, list[0].Id))!.Content;
        using var reader = new StreamReader(download);
        Assert.That(await reader.ReadToEndAsync(), Is.EqualTo("preuve"));

        Assert.That(await service.DeleteAsync(audit.Id, list[0].Id), Is.Null);
        Assert.That(await service.ListAsync(audit.Id), Is.Empty);
        Assert.That(await db.Documents.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Evidence_rejects_item_from_another_audit_and_workspace()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        await using var db = new FreecrmlanceDbContext(options);
        await db.Database.MigrateAsync();
        var ca = new Customer(a, "A"); var cb = new Customer(b, "B"); db.Customers.AddRange(ca, cb);
        var auditA = NewAudit(a, ca.Id); var auditB = NewAudit(b, cb.Id); db.Audits.AddRange(auditA, auditB); await db.SaveChangesAsync();

        var service = new AuditEvidenceService(db, new Stub(a), new MemoryStorage());
        await using var content = new MemoryStream([1, 2, 3]);
        var foreignItem = auditB.Sections.Single().Items.Single().Id;
        Assert.That(await service.UploadAsync(auditA.Id, foreignItem, "x.bin", "application/octet-stream", 3, content, "user"), Is.EqualTo("Critère d'audit introuvable."));
        Assert.That(await service.ListAsync(auditB.Id), Is.Null);
        Assert.That(await service.DownloadAsync(auditB.Id, Guid.NewGuid()), Is.Null);
        Assert.That(await service.DeleteAsync(auditB.Id, Guid.NewGuid()), Is.EqualTo("Audit introuvable."));
    }


    [Test]
    public async Task Evidence_rejects_oversized_file_before_storage()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        var wid = Guid.NewGuid(); await using var db = new FreecrmlanceDbContext(options); await db.Database.MigrateAsync();
        var customer = new Customer(wid, "Client"); db.Customers.Add(customer); var audit = NewAudit(wid, customer.Id); db.Audits.Add(audit); await db.SaveChangesAsync();
        var storage = new MemoryStorage(); var service = new AuditEvidenceService(db, new Stub(wid), storage); await using var content = new MemoryStream([1]);
        var error = await service.UploadAsync(audit.Id, audit.Sections.Single().Items.Single().Id, "big.bin", "application/octet-stream", AuditEvidenceService.MaxFileSize + 1, content, "user");
        Assert.That(error, Does.Contain("10 Mo")); Assert.That(storage.Count, Is.Zero); Assert.That(await db.Documents.CountAsync(), Is.Zero);
    }

    private static Audit NewAudit(Guid wid, Guid customerId)
    {
        var audit = new Audit(wid, customerId, Guid.NewGuid(), Guid.NewGuid().ToString("N"), "Audit", null);
        audit.AddSection("Section", 0).AddItem("Critère", AuditResponseType.Text, false, 0);
        return audit;
    }

    private sealed class Stub(Guid id) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken ct = default) => Task.FromResult<Guid?>(id);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken ct = default) => Task.FromResult(id);
    }

    private sealed class MemoryStorage : IFileStorage
    {
        private readonly Dictionary<string, byte[]> files = [];
        public int Count => files.Count;
        public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream(); await content.CopyToAsync(ms, cancellationToken); var key = Guid.NewGuid().ToString("N"); files[key] = ms.ToArray(); return key;
        }
        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(new MemoryStream(files[storageKey], writable: false));
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) { files.Remove(storageKey); return Task.CompletedTask; }
    }
}
