using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class DocumentService(
    FreecrmlanceDbContext dbContext,
    IWorkspaceContext workspaceContext,
    IFileStorage fileStorage) : IDocumentService
{
    public async Task<IReadOnlyList<DocumentDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customerExists = await dbContext.Customers.AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);
        if (!customerExists) return null;

        return await dbContext.Documents.AsNoTracking()
            .Where(document => document.WorkspaceId == workspaceId && document.CustomerId == customerId)
            .OrderByDescending(document => document.CreatedAtUtc)
            .Select(document => new DocumentDto(document.Id, document.FileName, document.ContentType, document.Size, document.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> UploadAsync(Guid customerId, string fileName, string contentType, long size, Stream content, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customerExists = await dbContext.Customers.AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);
        if (!customerExists) return null;

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName)) throw new ArgumentException("File name is required.", nameof(fileName));

        var storageKey = await fileStorage.SaveAsync(content, cancellationToken);
        try
        {
            var document = new Document(workspaceId, customerId, safeFileName, contentType, size, storageKey);
            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync(cancellationToken);
            return document.Id;
        }
        catch
        {
            await fileStorage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }

    public async Task<DocumentDownloadDto?> DownloadAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var document = await dbContext.Documents.AsNoTracking()
            .SingleOrDefaultAsync(
                document => document.Id == documentId
                    && document.CustomerId == customerId
                    && document.WorkspaceId == workspaceId,
                cancellationToken);

        if (document is null) return null;

        var content = await fileStorage.OpenReadAsync(document.StorageKey, cancellationToken);
        return new DocumentDownloadDto(document.FileName, document.ContentType, content);
    }
    public async Task<bool> DeleteAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var document = await dbContext.Documents.SingleOrDefaultAsync(
            document => document.Id == documentId
                && document.CustomerId == customerId
                && document.WorkspaceId == workspaceId,
            cancellationToken);

        if (document is null) return false;

        await fileStorage.DeleteAsync(document.StorageKey, cancellationToken);
        dbContext.Documents.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
