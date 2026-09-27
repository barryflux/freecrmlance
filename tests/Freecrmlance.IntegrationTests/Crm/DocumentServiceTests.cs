using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Crm;

public sealed class DocumentServiceTests
{
    [Test]
    public async Task Upload_list_and_download_are_scoped_by_workspace_and_customer()
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

        var storage = new MemoryFileStorage();
        var serviceA = new DocumentService(db, new StubWorkspaceContext(workspaceA), storage);
        await using var upload = new MemoryStream("hello"u8.ToArray());
        var documentId = await serviceA.UploadAsync(customerA.Id, "../../proposal.pdf", "application/pdf", upload.Length, upload);

        var listed = await serviceA.ListAsync(customerA.Id);
        var wrongCustomer = await serviceA.DownloadAsync(secondCustomerA.Id, documentId!.Value);
        var crossWorkspaceService = new DocumentService(db, new StubWorkspaceContext(workspaceB), storage);
        var crossWorkspace = await crossWorkspaceService.DownloadAsync(customerA.Id, documentId.Value);
        var downloaded = await serviceA.DownloadAsync(customerA.Id, documentId.Value);

        Assert.Multiple(() =>
        {
            Assert.That(listed, Has.Count.EqualTo(1));
            Assert.That(listed![0].FileName, Is.EqualTo("proposal.pdf"));
            Assert.That(wrongCustomer, Is.Null);
            Assert.That(crossWorkspace, Is.Null);
            Assert.That(downloaded, Is.Not.Null);
            Assert.That(downloaded!.FileName, Is.EqualTo("proposal.pdf"));
        });

        await downloaded!.Content.DisposeAsync();
    }


    [Test]
    public async Task Delete_removes_metadata_and_file_and_rejects_wrong_scope()
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

        var storage = new MemoryFileStorage();
        var serviceA = new DocumentService(db, new StubWorkspaceContext(workspaceA), storage);
        await using var upload = new MemoryStream("hello"u8.ToArray());
        var documentId = (await serviceA.UploadAsync(customerA.Id, "proposal.pdf", "application/pdf", upload.Length, upload))!.Value;

        var wrongCustomer = await serviceA.DeleteAsync(secondCustomerA.Id, documentId);
        var crossWorkspaceService = new DocumentService(db, new StubWorkspaceContext(workspaceB), storage);
        var crossWorkspace = await crossWorkspaceService.DeleteAsync(customerA.Id, documentId);
        var deleted = await serviceA.DeleteAsync(customerA.Id, documentId);

        Assert.Multiple(() =>
        {
            Assert.That(wrongCustomer, Is.False);
            Assert.That(crossWorkspace, Is.False);
            Assert.That(deleted, Is.True);
            Assert.That(db.Documents.Any(document => document.Id == documentId), Is.False);
            Assert.That(storage.Count, Is.Zero);
        });
    }

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(workspaceId);
        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(workspaceId);
    }

    private sealed class MemoryFileStorage : IFileStorage
    {
        private readonly Dictionary<string, byte[]> files = [];

        public int Count => files.Count;

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
