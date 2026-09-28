using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class DocumentShareServiceTests
{
    [Test]
    public async Task Share_download_and_revoke_are_secure_and_tenant_scoped()
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

        var storage = new MemoryFileStorage();
        var documentService = new DocumentService(db, new StubWorkspaceContext(workspaceA), storage);
        await using var upload = new MemoryStream("shared content"u8.ToArray());
        var documentId = (await documentService.UploadAsync(customerA.Id, "proposal.pdf", "application/pdf", upload.Length, upload))!.Value;

        var serviceA = new DocumentShareService(db, new StubWorkspaceContext(workspaceA), storage);
        var serviceB = new DocumentShareService(db, new StubWorkspaceContext(workspaceB), storage);

        var crossWorkspaceCreate = await serviceB.CreateAsync(customerA.Id, documentId);
        var share = await serviceA.CreateAsync(customerA.Id, documentId);
        var duplicateShare = await serviceA.CreateAsync(customerA.Id, documentId);
        var persisted = await db.DocumentShares.AsNoTracking().SingleAsync();
        var invalid = await serviceA.DownloadAsync("invalid-token");
        var downloaded = await serviceA.DownloadAsync(share!.Token);
        var crossWorkspaceRevoke = await serviceB.RevokeAsync(customerA.Id, documentId, share.Id);
        var revoked = await serviceA.RevokeAsync(customerA.Id, documentId, share.Id);
        var afterRevoke = await serviceA.DownloadAsync(share.Token);

        Assert.Multiple(() =>
        {
            Assert.That(crossWorkspaceCreate, Is.Null);
            Assert.That(share.Token, Has.Length.EqualTo(64));
            Assert.That(duplicateShare, Is.Null);
            Assert.That(persisted.TokenHash, Is.Not.EqualTo(share.Token));
            Assert.That(invalid, Is.Null);
            Assert.That(downloaded, Is.Not.Null);
            Assert.That(crossWorkspaceRevoke, Is.False);
            Assert.That(revoked, Is.True);
            Assert.That(afterRevoke, Is.Null);
        });

        await downloaded!.Content.DisposeAsync();
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }

    private sealed class MemoryFileStorage : IFileStorage
    {
        private readonly Dictionary<string, byte[]> files = [];

        public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default)
        {
            var key = Guid.NewGuid().ToString("N");
            await using var copy = new MemoryStream();
            await content.CopyToAsync(copy, cancellationToken);
            files[key] = copy.ToArray();
            return key;
        }

        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(files[storageKey], writable: false));

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            files.Remove(storageKey);
            return Task.CompletedTask;
        }
    }
}
